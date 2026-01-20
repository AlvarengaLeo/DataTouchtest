using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Service for managing appointments/bookings.
/// </summary>
public class AppointmentService
{
    private readonly DataTouchDbContext _db;
    private readonly AvailabilityService _availabilityService;

    public AppointmentService(DataTouchDbContext db, AvailabilityService availabilityService)
    {
        _db = db;
        _availabilityService = availabilityService;
    }

    // ═══════════════════════════════════════════════════════════════
    // PUBLIC API (No auth required - called from public booking page)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get active services for a card (public view).
    /// </summary>
    public async Task<List<Service>> GetPublicServicesAsync(Guid cardId)
    {
        return await _db.Services
            .Where(s => s.CardId == cardId && s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Get available time slots for a specific date.
    /// </summary>
    public async Task<List<TimeSlot>> GetAvailableSlotsAsync(Guid cardId, DateOnly date, Guid? serviceId = null)
    {
        // Get service duration (default 30 min if no service specified)
        var durationMinutes = 30;
        if (serviceId.HasValue)
        {
            var service = await _db.Services.FindAsync(serviceId.Value);
            if (service != null) durationMinutes = service.DurationMinutes;
        }

        // Get all possible slots from availability service
        var possibleSlots = await _availabilityService.CalculateSlotsForDateAsync(cardId, date, durationMinutes);

        // Get existing appointments for this date to filter out booked slots
        var dateStart = date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue);
        
        var bookedAppointments = await _db.Appointments
            .Where(a => a.CardId == cardId && 
                        a.StartDateTime >= dateStart && 
                        a.StartDateTime <= dateEnd &&
                        a.Status != AppointmentStatus.Cancelled)
            .Select(a => new { a.StartDateTime, a.EndDateTime })
            .ToListAsync();

        // Filter out slots that overlap with booked appointments
        var availableSlots = possibleSlots.Where(slot =>
        {
            var slotStart = date.ToDateTime(slot.StartTime);
            var slotEnd = date.ToDateTime(slot.EndTime);
            return !bookedAppointments.Any(booked =>
                slotStart < booked.EndDateTime && slotEnd > booked.StartDateTime);
        }).ToList();

        return availableSlots;
    }

    /// <summary>
    /// Create an appointment from public booking (with concurrency check).
    /// </summary>
    public async Task<AppointmentResult> CreatePublicAppointmentAsync(CreateAppointmentDto dto)
    {
        // Get card and validate it exists
        var card = await _db.Cards
            .Include(c => c.Organization)
            .FirstOrDefaultAsync(c => c.Id == dto.CardId);
        
        if (card == null)
            return new AppointmentResult { Success = false, Error = "Tarjeta no encontrada" };

        // Validate service if specified
        Service? service = null;
        var durationMinutes = 30;
        if (dto.ServiceId.HasValue)
        {
            service = await _db.Services.FindAsync(dto.ServiceId.Value);
            if (service != null) durationMinutes = service.DurationMinutes;
        }

        var startDateTime = dto.Date.ToDateTime(dto.StartTime);
        var endDateTime = startDateTime.AddMinutes(durationMinutes);

        // CONCURRENCY CHECK: Ensure slot is still available
        var conflictExists = await _db.Appointments
            .AnyAsync(a => a.CardId == dto.CardId &&
                          a.Status != AppointmentStatus.Cancelled &&
                          a.StartDateTime < endDateTime &&
                          a.EndDateTime > startDateTime);

        if (conflictExists)
            return new AppointmentResult { Success = false, Error = "Este horario ya no está disponible. Por favor selecciona otro." };

        // Create the appointment
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CardId = dto.CardId,
            OrganizationId = card.OrganizationId,
            ServiceId = dto.ServiceId,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            Timezone = "America/El_Salvador",
            Status = dto.InitialStatus, // Use DTO status for manual creation
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            CustomerPhoneCountryCode = dto.CustomerPhoneCountryCode,
            CustomerNotes = dto.CustomerNotes,
            Source = dto.Source, // Use DTO source (PublicCard or Manual)
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        
        try
        {
            await _db.SaveChangesAsync();
            return new AppointmentResult 
            { 
                Success = true, 
                Appointment = appointment,
                Message = "¡Cita reservada exitosamente!" 
            };
        }
        catch (DbUpdateException)
        {
            // Race condition - another request created an overlapping appointment
            return new AppointmentResult { Success = false, Error = "Este horario ya no está disponible. Por favor selecciona otro." };
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CRM API (Auth required - admin management)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get appointments for CRM view with filtering.
    /// </summary>
    public async Task<List<Appointment>> GetAppointmentsAsync(
        Guid cardId, 
        DateTime? fromDate = null, 
        DateTime? toDate = null,
        AppointmentStatus? status = null)
    {
        var query = _db.Appointments
            .Include(a => a.Service)
            .Where(a => a.CardId == cardId);

        if (fromDate.HasValue)
            query = query.Where(a => a.StartDateTime >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(a => a.StartDateTime <= toDate.Value);
        
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.StartDateTime)
            .ToListAsync();
    }

    /// <summary>
    /// Update appointment status.
    /// </summary>
    public async Task<Appointment?> UpdateStatusAsync(Guid appointmentId, AppointmentStatus newStatus)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment == null) return null;

        appointment.Status = newStatus;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return appointment;
    }

    /// <summary>
    /// Reschedule an appointment.
    /// </summary>
    public async Task<AppointmentResult> RescheduleAsync(Guid appointmentId, DateTime newStart)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
        
        if (appointment == null)
            return new AppointmentResult { Success = false, Error = "Cita no encontrada" };

        var durationMinutes = appointment.Service?.DurationMinutes ?? 30;
        var newEnd = newStart.AddMinutes(durationMinutes);

        // Check for conflicts
        var conflictExists = await _db.Appointments
            .AnyAsync(a => a.CardId == appointment.CardId &&
                          a.Id != appointmentId &&
                          a.Status != AppointmentStatus.Cancelled &&
                          a.StartDateTime < newEnd &&
                          a.EndDateTime > newStart);

        if (conflictExists)
            return new AppointmentResult { Success = false, Error = "El nuevo horario no está disponible" };

        appointment.StartDateTime = newStart;
        appointment.EndDateTime = newEnd;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AppointmentResult { Success = true, Appointment = appointment };
    }

    /// <summary>
    /// Add internal notes to appointment.
    /// </summary>
    public async Task UpdateNotesAsync(Guid appointmentId, string notes)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment != null)
        {
            appointment.InternalNotes = notes;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
    
    // ═══════════════════════════════════════════════════════════════
    // CANCEL WITH REASON (Supports Undo)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Cancel appointment with reason, storing previous state for undo.
    /// </summary>
    public async Task<AppointmentResult> CancelWithReasonAsync(Guid appointmentId, string reason, Guid? cancelledByUserId = null)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment == null)
        {
            return new AppointmentResult { Success = false, Error = "Cita no encontrada" };
        }
        
        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return new AppointmentResult { Success = false, Error = "La cita ya está cancelada" };
        }
        
        // Store previous status for undo
        appointment.PreviousStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.CancelledByUserId = cancelledByUserId;
        appointment.CancelReason = reason;
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        return new AppointmentResult 
        { 
            Success = true, 
            Appointment = appointment,
            Message = "Cita cancelada exitosamente"
        };
    }
    
    // ═══════════════════════════════════════════════════════════════
    // RESTORE (With Overlap Validation)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Restore a cancelled appointment, validating no overlap with active appointments.
    /// </summary>
    public async Task<AppointmentResult> RestoreAsync(Guid appointmentId)
    {
        var appointment = await _db.Appointments.FindAsync(appointmentId);
        if (appointment == null)
        {
            return new AppointmentResult { Success = false, Error = "Cita no encontrada" };
        }
        
        if (appointment.Status != AppointmentStatus.Cancelled)
        {
            return new AppointmentResult { Success = false, Error = "Solo citas canceladas pueden ser restauradas" };
        }
        
        // Check for overlap with active appointments (Pending, Confirmed, Rescheduled)
        var hasOverlap = await _db.Appointments.AnyAsync(a =>
            a.CardId == appointment.CardId &&
            a.Id != appointment.Id &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Completed &&
            a.StartDateTime < appointment.EndDateTime &&
            a.EndDateTime > appointment.StartDateTime);
        
        if (hasOverlap)
        {
            return new AppointmentResult 
            { 
                Success = false, 
                Error = "El horario ya fue ocupado por otra cita. Reprograme la cita."
            };
        }
        
        // Restore to previous status (or Pending if not stored)
        appointment.Status = appointment.PreviousStatus ?? AppointmentStatus.Pending;
        appointment.PreviousStatus = null;
        appointment.CancelledAt = null;
        appointment.CancelledByUserId = null;
        appointment.CancelReason = null;
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        return new AppointmentResult 
        { 
            Success = true, 
            Appointment = appointment,
            Message = "Cita restaurada exitosamente"
        };
    }
}

// ═══════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════

public class TimeSlot
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    
    public string DisplayTime => StartTime.ToString("h:mm tt");
}

public class CreateAppointmentDto
{
    public Guid CardId { get; set; }
    public Guid? ServiceId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public string? CustomerNotes { get; set; }
    
    // For manual CRM creation
    public AppointmentStatus InitialStatus { get; set; } = AppointmentStatus.Pending;
    public string Source { get; set; } = "PublicCard";
}

public class AppointmentResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public Appointment? Appointment { get; set; }
}
