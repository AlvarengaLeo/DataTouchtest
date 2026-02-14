using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Business logic for reservation requests (reservations-range template).
/// </summary>
public class ReservationService
{
    private readonly IDbContextFactory<DataTouchDbContext> _factory;

    public ReservationService(IDbContextFactory<DataTouchDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Submit a new reservation request from the public card.
    /// </summary>
    public async Task<ReservationRequest> SubmitReservationAsync(
        Guid cardId, Guid organizationId,
        DateTime fromDate, DateTime toDate,
        int guestsAdults, int guestsChildren,
        Guid? resourceId,
        string? extrasJson,
        string contactName, string? contactPhone, string? contactPhoneCountryCode,
        string? contactEmail, string? notes,
        string? ipAddress, string? userAgent, string? idempotencyKey)
    {
        await using var db = await _factory.CreateDbContextAsync();

        // Idempotency check
        if (!string.IsNullOrEmpty(idempotencyKey))
        {
            var existing = await db.ReservationRequests
                .FirstOrDefaultAsync(r => r.IdempotencyKey == idempotencyKey);
            if (existing != null) return existing;
        }

        // Generate request number
        var count = await db.ReservationRequests
            .Where(r => r.OrganizationId == organizationId)
            .CountAsync();
        var requestNumber = $"RR-{DateTime.UtcNow:yyyy}-{(count + 1):D4}";

        var nights = (int)(toDate.Date - fromDate.Date).TotalDays;
        if (nights < 1) nights = 1;

        var reservation = new ReservationRequest
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            CardId = cardId,
            ResourceId = resourceId,
            RequestNumber = requestNumber,
            FromDate = fromDate.Date,
            ToDate = toDate.Date,
            Nights = nights,
            GuestsAdults = guestsAdults,
            GuestsChildren = guestsChildren,
            ExtrasJson = extrasJson,
            Notes = notes,
            ContactName = contactName,
            ContactPhone = contactPhone,
            ContactPhoneCountryCode = contactPhoneCountryCode,
            ContactEmail = contactEmail,
            Status = ReservationStatus.New,
            Source = "PublicCard",
            TemplateKey = "ReservationsRange",
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };

        db.ReservationRequests.Add(reservation);
        await db.SaveChangesAsync();

        return reservation;
    }

    /// <summary>
    /// Get all reservation requests for a card.
    /// </summary>
    public async Task<List<ReservationRequest>> GetReservationsAsync(Guid cardId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.ReservationRequests
            .Where(r => r.CardId == cardId)
            .Include(r => r.Resource)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Get reservations for an organization.
    /// </summary>
    public async Task<List<ReservationRequest>> GetByOrganizationAsync(Guid organizationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.ReservationRequests
            .Where(r => r.OrganizationId == organizationId)
            .Include(r => r.Resource)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Update reservation status.
    /// </summary>
    public async Task<bool> UpdateStatusAsync(Guid reservationId, ReservationStatus newStatus, string? reason = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var reservation = await db.ReservationRequests.FindAsync(reservationId);
        if (reservation == null) return false;

        reservation.Status = newStatus;
        reservation.StatusReason = reason;
        reservation.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get blocked dates for a resource (or all resources of a card).
    /// Returns dates that have confirmed reservations overlapping them.
    /// </summary>
    public async Task<HashSet<DateTime>> GetBlockedDatesAsync(Guid cardId, Guid? resourceId, DateTime monthStart, DateTime monthEnd)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var blocked = new HashSet<DateTime>();

        // Get confirmed reservations that overlap the month
        var query = db.ReservationRequests
            .Where(r => r.CardId == cardId
                && r.Status != ReservationStatus.Cancelled
                && r.FromDate < monthEnd
                && r.ToDate > monthStart);

        if (resourceId.HasValue)
            query = query.Where(r => r.ResourceId == resourceId.Value);

        var reservations = await query.ToListAsync();

        foreach (var r in reservations)
        {
            for (var d = r.FromDate.Date; d < r.ToDate.Date; d = d.AddDays(1))
            {
                if (d >= monthStart.Date && d < monthEnd.Date)
                    blocked.Add(d);
            }
        }

        // Also check resource-level blocked dates
        if (resourceId.HasValue)
        {
            var resource = await db.ReservationResources.FindAsync(resourceId.Value);
            if (resource != null && !string.IsNullOrEmpty(resource.BlockedDatesJson))
            {
                try
                {
                    var dates = System.Text.Json.JsonSerializer.Deserialize<List<string>>(resource.BlockedDatesJson);
                    if (dates != null)
                    {
                        foreach (var ds in dates)
                        {
                            if (DateTime.TryParse(ds, out var dt) && dt >= monthStart && dt < monthEnd)
                                blocked.Add(dt.Date);
                        }
                    }
                }
                catch { /* ignore malformed JSON */ }
            }
        }

        return blocked;
    }
}
