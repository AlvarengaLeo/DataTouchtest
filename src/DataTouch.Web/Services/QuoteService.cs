using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Enterprise service for managing quote requests (cotizaciones).
/// Supports idempotency, lead deduplication, and activity logging.
/// </summary>
public class QuoteService
{
    private readonly DataTouchDbContext _db;

    public QuoteService(DataTouchDbContext db)
    {
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC API (No auth required - called from public card)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a quote request from public card.
    /// Supports idempotency and lead deduplication.
    /// </summary>
    public async Task<QuoteResult> CreatePublicQuoteAsync(CreateQuoteDto dto)
    {
        // Idempotency check - prevent duplicate submissions
        if (!string.IsNullOrEmpty(dto.IdempotencyKey))
        {
            var existing = await _db.QuoteRequests
                .FirstOrDefaultAsync(q => q.IdempotencyKey == dto.IdempotencyKey);
            
            if (existing != null)
            {
                return new QuoteResult
                {
                    Success = true,
                    Quote = existing,
                    Message = "Solicitud ya procesada",
                    IsDuplicate = true
                };
            }
        }

        // Validate card exists
        var card = await _db.Cards
            .Include(c => c.Organization)
            .FirstOrDefaultAsync(c => c.Id == dto.CardId);

        if (card == null)
            return new QuoteResult { Success = false, Error = "Tarjeta no encontrada" };

        // Validate service exists and is active (skip if no service)
        Service? service = null;
        if (dto.ServiceId != Guid.Empty)
        {
            service = await _db.Services
                .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.CardId == dto.CardId && s.IsActive);

            if (service == null)
                return new QuoteResult { Success = false, Error = "Servicio no disponible" };
        }

        // Lead deduplication - find or create lead
        var lead = await FindOrCreateLeadAsync(
            card.OrganizationId, 
            dto.CustomerEmail, 
            dto.CustomerName, 
            dto.CustomerPhone,
            dto.CustomerPhoneCountryCode,
            card.Id);

        // Generate request number
        var requestNumber = await GenerateRequestNumberAsync(card.OrganizationId);

        // Calculate SLA deadline (default: 24 hours)
        var slaDeadline = DateTime.UtcNow.AddHours(24);

        // Create quote request
        var quote = new QuoteRequest
        {
            Id = Guid.NewGuid(),
            OrganizationId = card.OrganizationId,
            CardId = dto.CardId,
            ServiceId = dto.ServiceId,
            LeadId = lead.Id,
            RequestNumber = requestNumber,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            CustomerPhoneCountryCode = dto.CustomerPhoneCountryCode,
            Description = dto.Description,
            Status = QuoteStatus.New,
            Priority = 2, // Medium by default
            SlaDeadlineAt = slaDeadline,
            IdempotencyKey = dto.IdempotencyKey ?? Guid.NewGuid().ToString(),
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CreatedAt = DateTime.UtcNow
        };

        _db.QuoteRequests.Add(quote);

        // Create activity record
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OrganizationId = card.OrganizationId,
            EntityType = "QuoteRequest",
            EntityId = quote.Id,
            Type = ActivityType.Created,
            Description = service != null ? $"Solicitud de cotización creada para {service.Name}" : "Solicitud de cotización creada",
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                serviceName = service.Name,
                customerEmail = dto.CustomerEmail,
                source = "public_card"
            }),
            CreatedAt = DateTime.UtcNow
        };
        _db.Activities.Add(activity);

        await _db.SaveChangesAsync();

        return new QuoteResult
        {
            Success = true,
            Quote = quote,
            RequestNumber = requestNumber,
            Message = "¡Solicitud de cotización enviada!"
        };
    }

    /// <summary>
    /// Find existing lead by email or create new one.
    /// </summary>
    private async Task<Lead> FindOrCreateLeadAsync(
        Guid orgId, 
        string email, 
        string fullName, 
        string? phone,
        string? phoneCountryCode,
        Guid sourceCardId)
    {
        var existingLead = await _db.Leads
            .FirstOrDefaultAsync(l => l.OrganizationId == orgId && l.Email == email);

        if (existingLead != null)
        {
            // Update name if the new one is more complete
            if (!string.IsNullOrWhiteSpace(fullName) && 
                fullName.Length > (existingLead.FullName?.Length ?? 0))
            {
                existingLead.FullName = fullName;
            }

            // Update phone if not set
            if (string.IsNullOrEmpty(existingLead.Phone) && !string.IsNullOrEmpty(phone))
            {
                existingLead.Phone = phone;
                existingLead.PhoneCountryCode = phoneCountryCode;
            }

            existingLead.LastActivityAt = DateTime.UtcNow;
            return existingLead;
        }

        // Create new lead
        var newLead = new Lead
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Email = email,
            FullName = fullName,
            Phone = phone,
            PhoneCountryCode = phoneCountryCode,
            Source = "quote_form",
            CardId = sourceCardId,
            OwnerUserId = Guid.Empty, // Will be assigned when admin reviews
            Status = "new",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        _db.Leads.Add(newLead);
        return newLead;
    }

    /// <summary>
    /// Generate unique request number: QR-YYYY-NNNN
    /// </summary>
    private async Task<string> GenerateRequestNumberAsync(Guid orgId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"QR-{year}-";
        
        var lastNumber = await _db.QuoteRequests
            .Where(q => q.OrganizationId == orgId && q.RequestNumber.StartsWith(prefix))
            .OrderByDescending(q => q.RequestNumber)
            .Select(q => q.RequestNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastNumber != null)
        {
            var numPart = lastNumber.Replace(prefix, "");
            if (int.TryParse(numPart, out int parsed))
                nextNumber = parsed + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC API — Quote Request Template (no service catalog)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a quote request from the public QuoteRequest template card.
    /// Unlike CreatePublicQuoteAsync, this does NOT require a service — it's a general request.
    /// </summary>
    public async Task<QuoteResult> CreateQuoteFromPublicCardRequestAsync(CreatePublicCardQuoteDto dto)
    {
        // Idempotency check
        if (!string.IsNullOrEmpty(dto.IdempotencyKey))
        {
            var existing = await _db.QuoteRequests
                .FirstOrDefaultAsync(q => q.IdempotencyKey == dto.IdempotencyKey);
            if (existing != null)
            {
                return new QuoteResult
                {
                    Success = true,
                    Quote = existing,
                    Message = "Solicitud ya procesada",
                    IsDuplicate = true
                };
            }
        }

        // Validate card exists
        var card = await _db.Cards
            .Include(c => c.Organization)
            .FirstOrDefaultAsync(c => c.Id == dto.CardId);

        if (card == null)
            return new QuoteResult { Success = false, Error = "Tarjeta no encontrada" };

        // Lead deduplication — by email if available, otherwise by phone
        var lead = await FindOrCreateLeadForPublicRequestAsync(
            card.OrganizationId,
            dto.CustomerEmail,
            dto.CustomerName,
            dto.CustomerPhone,
            dto.CustomerPhoneCountryCode,
            card.Id,
            card.UserId);

        // Generate request number
        var requestNumber = await GenerateRequestNumberAsync(card.OrganizationId);

        // SLA deadline (default: 24 hours)
        var slaDeadline = DateTime.UtcNow.AddHours(24);

        // Build custom fields metadata
        var customFields = System.Text.Json.JsonSerializer.Serialize(new
        {
            source = "PublicCard",
            budget = dto.Budget,
            deadline = dto.Deadline,
            preferredContact = dto.PreferredContact
        });

        // Create quote request (no ServiceId)
        var quote = new QuoteRequest
        {
            Id = Guid.NewGuid(),
            OrganizationId = card.OrganizationId,
            CardId = dto.CardId,
            ServiceId = null,
            LeadId = lead.Id,
            RequestNumber = requestNumber,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            CustomerPhoneCountryCode = dto.CustomerPhoneCountryCode,
            Description = dto.Details,
            CustomFieldsJson = customFields,
            Status = QuoteStatus.New,
            Priority = 2,
            SlaDeadlineAt = slaDeadline,
            IdempotencyKey = dto.IdempotencyKey ?? Guid.NewGuid().ToString(),
            IpAddress = dto.IpAddress,
            UserAgent = dto.UserAgent,
            Referrer = dto.Referrer,
            CreatedAt = DateTime.UtcNow
        };

        _db.QuoteRequests.Add(quote);

        // Activity log
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OrganizationId = card.OrganizationId,
            EntityType = "QuoteRequest",
            EntityId = quote.Id,
            Type = ActivityType.Created,
            Description = "Solicitud de cotización desde tarjeta pública",
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                source = "public_card_quote_template",
                customerEmail = dto.CustomerEmail,
                customerPhone = dto.CustomerPhone
            }),
            CreatedAt = DateTime.UtcNow
        };
        _db.Activities.Add(activity);

        await _db.SaveChangesAsync();

        return new QuoteResult
        {
            Success = true,
            Quote = quote,
            RequestNumber = requestNumber,
            Message = "¡Solicitud de cotización enviada!"
        };
    }

    /// <summary>
    /// Find or create lead for public card request.
    /// Deduplicates by email (primary) or phone (fallback when email is null).
    /// </summary>
    private async Task<Lead> FindOrCreateLeadForPublicRequestAsync(
        Guid orgId,
        string? email,
        string fullName,
        string? phone,
        string? phoneCountryCode,
        Guid sourceCardId,
        Guid? ownerUserId = null)
    {
        Lead? existingLead = null;

        // Try email dedup first
        if (!string.IsNullOrEmpty(email))
        {
            existingLead = await _db.Leads
                .FirstOrDefaultAsync(l => l.OrganizationId == orgId && l.Email == email);
        }
        // Fallback: dedup by phone if no email
        else if (!string.IsNullOrEmpty(phone))
        {
            existingLead = await _db.Leads
                .FirstOrDefaultAsync(l => l.OrganizationId == orgId && l.Phone == phone);
        }

        if (existingLead != null)
        {
            if (!string.IsNullOrWhiteSpace(fullName) &&
                fullName.Length > (existingLead.FullName?.Length ?? 0))
            {
                existingLead.FullName = fullName;
            }
            if (string.IsNullOrEmpty(existingLead.Phone) && !string.IsNullOrEmpty(phone))
            {
                existingLead.Phone = phone;
                existingLead.PhoneCountryCode = phoneCountryCode;
            }
            if (string.IsNullOrEmpty(existingLead.Email) && !string.IsNullOrEmpty(email))
            {
                existingLead.Email = email;
            }
            existingLead.LastActivityAt = DateTime.UtcNow;
            return existingLead;
        }

        // Create new lead
        var newLead = new Lead
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Email = email ?? "",
            FullName = fullName,
            Phone = phone,
            PhoneCountryCode = phoneCountryCode,
            Source = "quote_request_template",
            CardId = sourceCardId,
            OwnerUserId = ownerUserId ?? Guid.Empty,
            Status = "new",
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        _db.Leads.Add(newLead);
        return newLead;
    }

    // ═══════════════════════════════════════════════════════════════
    // CRM API (Auth required)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get quote requests for CRM view with includes.
    /// </summary>
    public async Task<List<QuoteRequest>> GetQuotesAsync(
        Guid orgId, 
        QuoteStatus? status = null,
        Guid? serviceId = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = _db.QuoteRequests
            .Include(q => q.Service)
            .Where(q => q.OrganizationId == orgId);

        if (status.HasValue)
            query = query.Where(q => q.Status == status.Value);

        if (serviceId.HasValue)
            query = query.Where(q => q.ServiceId == serviceId.Value);

        // Exclude archived by default
        query = query.Where(q => q.Status != QuoteStatus.Archived);

        return await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Get count by status for dashboard.
    /// </summary>
    public async Task<Dictionary<QuoteStatus, int>> GetStatusCountsAsync(Guid orgId)
    {
        return await _db.QuoteRequests
            .Where(q => q.OrganizationId == orgId && q.Status != QuoteStatus.Archived)
            .GroupBy(q => q.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    /// <summary>
    /// Update quote status with activity logging.
    /// </summary>
    public async Task<QuoteRequest?> UpdateStatusAsync(
        Guid quoteId, 
        QuoteStatus newStatus, 
        string? reason = null,
        Guid? userId = null)
    {
        var quote = await _db.QuoteRequests.FindAsync(quoteId);
        if (quote == null) return null;

        var oldStatus = quote.Status;
        quote.Status = newStatus;
        quote.StatusReason = reason;
        quote.UpdatedAt = DateTime.UtcNow;

        // Set timestamps based on status
        if (newStatus == QuoteStatus.Won)
            quote.WonAt = DateTime.UtcNow;
        else if (newStatus == QuoteStatus.Lost)
            quote.LostAt = DateTime.UtcNow;

        // Log activity
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OrganizationId = quote.OrganizationId,
            EntityType = "QuoteRequest",
            EntityId = quote.Id,
            Type = ActivityType.StatusChange,
            Description = $"Estado cambiado de {oldStatus} a {newStatus}",
            MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                oldStatus = oldStatus.ToString(),
                newStatus = newStatus.ToString(),
                reason
            }),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Activities.Add(activity);

        await _db.SaveChangesAsync();

        return quote;
    }

    /// <summary>
    /// Assign owner to quote.
    /// </summary>
    public async Task<QuoteRequest?> AssignOwnerAsync(Guid quoteId, Guid ownerId, Guid? assignedBy = null)
    {
        var quote = await _db.QuoteRequests.FindAsync(quoteId);
        if (quote == null) return null;

        quote.OwnerId = ownerId;
        quote.UpdatedAt = DateTime.UtcNow;

        // Move to InReview if New
        if (quote.Status == QuoteStatus.New)
            quote.Status = QuoteStatus.InReview;

        // Log activity
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OrganizationId = quote.OrganizationId,
            EntityType = "QuoteRequest",
            EntityId = quote.Id,
            Type = ActivityType.Assignment,
            Description = "Responsable asignado",
            UserId = assignedBy,
            CreatedAt = DateTime.UtcNow
        };
        _db.Activities.Add(activity);

        await _db.SaveChangesAsync();

        return quote;
    }

    /// <summary>
    /// Add internal notes to quote.
    /// </summary>
    public async Task AddNoteAsync(Guid quoteId, string note, Guid? userId = null)
    {
        var quote = await _db.QuoteRequests.FindAsync(quoteId);
        if (quote == null) return;

        quote.InternalNotes = string.IsNullOrEmpty(quote.InternalNotes) 
            ? note 
            : $"{quote.InternalNotes}\n---\n{note}";
        quote.UpdatedAt = DateTime.UtcNow;

        // Log activity
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OrganizationId = quote.OrganizationId,
            EntityType = "QuoteRequest",
            EntityId = quote.Id,
            Type = ActivityType.Note,
            Description = note.Length > 100 ? note[..100] + "..." : note,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Activities.Add(activity);

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Get activity timeline for a quote.
    /// </summary>
    public async Task<List<Activity>> GetTimelineAsync(Guid quoteId)
    {
        return await _db.Activities
            .Where(a => a.EntityType == "QuoteRequest" && a.EntityId == quoteId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Convert quote to appointment (when customer accepts).
    /// </summary>
    public async Task<AppointmentResult> ConvertToAppointmentAsync(
        Guid quoteId, 
        DateOnly date, 
        TimeOnly startTime, 
        Guid? userId = null)
    {
        var quote = await _db.QuoteRequests
            .Include(q => q.Card)
            .Include(q => q.Service)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null)
            return new AppointmentResult { Success = false, Error = "Cotización no encontrada" };

        // Get duration from service
        var durationMinutes = quote.Service?.DurationMinutes ?? 30;

        var startDateTime = date.ToDateTime(startTime);
        var endDateTime = startDateTime.AddMinutes(durationMinutes);

        // Check for conflicts
        var conflictExists = await _db.Appointments
            .AnyAsync(a => a.CardId == quote.CardId &&
                          a.Status != AppointmentStatus.Cancelled &&
                          a.StartDateTime < endDateTime &&
                          a.EndDateTime > startDateTime);

        if (conflictExists)
            return new AppointmentResult { Success = false, Error = "El horario seleccionado no está disponible" };

        // Create appointment
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CardId = quote.CardId,
            OrganizationId = quote.OrganizationId,
            ServiceId = quote.ServiceId,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            Timezone = "America/El_Salvador",
            Status = AppointmentStatus.Pending,
            CustomerName = quote.CustomerName,
            CustomerEmail = quote.CustomerEmail ?? "",
            CustomerPhone = quote.CustomerPhone,
            CustomerPhoneCountryCode = quote.CustomerPhoneCountryCode,
            CustomerNotes = $"Convertido desde cotización {quote.RequestNumber}. {quote.Description}",
            Source = "Quote",
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);

        // Update quote status and link to appointment
        quote.Status = QuoteStatus.Won;
        quote.ConvertedAppointmentId = appointment.Id;
        quote.WonAt = DateTime.UtcNow;
        quote.UpdatedAt = DateTime.UtcNow;

        // Log activity
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            OrganizationId = quote.OrganizationId,
            EntityType = "QuoteRequest",
            EntityId = quote.Id,
            Type = ActivityType.Conversion,
            Description = $"Convertida a cita para {date:dd/MM/yyyy} a las {startTime:HH:mm}",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Activities.Add(activity);

        await _db.SaveChangesAsync();

        return new AppointmentResult
        {
            Success = true,
            Appointment = appointment,
            Message = "Cotización convertida a cita exitosamente"
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════

public class CreateQuoteDto
{
    public Guid CardId { get; set; }
    public Guid ServiceId { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public string? Description { get; set; }
    
    // Idempotency & tracking
    public string? IdempotencyKey { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
}

public class CreatePublicCardQuoteDto
{
    public Guid CardId { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public required string Details { get; set; }
    public string? Budget { get; set; }
    public string? Deadline { get; set; }
    public string? PreferredContact { get; set; }

    // Tracking
    public string? IdempotencyKey { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
}

public class QuoteResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public string? RequestNumber { get; set; }
    public QuoteRequest? Quote { get; set; }
    public bool IsDuplicate { get; set; }
}
