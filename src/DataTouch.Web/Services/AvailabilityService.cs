using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Service for managing availability rules and calculating slots.
/// </summary>
public class AvailabilityService
{
    private readonly DataTouchDbContext _db;
    
    // Default buffer between appointments (minutes)
    private const int DefaultBufferMinutes = 0;
    
    // Default appointment duration if not specified
    private const int DefaultDurationMinutes = 30;

    public AvailabilityService(DataTouchDbContext db)
    {
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════════
    // SLOT CALCULATION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate all possible time slots for a specific date based on availability rules.
    /// </summary>
    public async Task<List<TimeSlot>> CalculateSlotsForDateAsync(Guid cardId, DateOnly date, int durationMinutes = DefaultDurationMinutes)
    {
        var slots = new List<TimeSlot>();
        var dayOfWeek = (int)date.DayOfWeek;

        // Get availability rule for this day
        var rule = await _db.AvailabilityRules
            .FirstOrDefaultAsync(r => r.CardId == cardId && r.DayOfWeek == dayOfWeek && r.IsActive);

        // Check for exceptions on this date
        var exceptions = await _db.AvailabilityExceptions
            .Where(e => e.CardId == cardId && e.ExceptionDate == date)
            .ToListAsync();

        // If there's a "Blocked" exception for the whole day, no slots available
        if (exceptions.Any(e => e.ExceptionType == AvailabilityExceptionType.Blocked && 
                               e.StartTime == null && e.EndTime == null))
        {
            return slots; // Empty - day is blocked
        }

        // Collect all time windows
        var timeWindows = new List<(TimeSpan Start, TimeSpan End)>();

        // Add regular rule if exists
        if (rule != null)
        {
            timeWindows.Add((rule.StartTime, rule.EndTime));
        }

        // Add extra hours from exceptions
        foreach (var ex in exceptions.Where(e => e.ExceptionType == AvailabilityExceptionType.ExtraHours))
        {
            if (ex.StartTime.HasValue && ex.EndTime.HasValue)
            {
                timeWindows.Add((ex.StartTime.Value, ex.EndTime.Value));
            }
        }

        // Remove blocked time ranges
        var blockedRanges = exceptions
            .Where(e => e.ExceptionType == AvailabilityExceptionType.Blocked && 
                       e.StartTime.HasValue && e.EndTime.HasValue)
            .Select(e => (Start: e.StartTime!.Value, End: e.EndTime!.Value))
            .ToList();

        // Generate slots from each time window
        foreach (var window in timeWindows)
        {
            var slotStart = window.Start;
            while (slotStart.Add(TimeSpan.FromMinutes(durationMinutes)) <= window.End)
            {
                var slotEnd = slotStart.Add(TimeSpan.FromMinutes(durationMinutes));
                
                // Check if this slot overlaps with any blocked range
                var isBlocked = blockedRanges.Any(blocked =>
                    slotStart < blocked.End && slotEnd > blocked.Start);

                if (!isBlocked)
                {
                    slots.Add(new TimeSlot
                    {
                        StartTime = TimeOnly.FromTimeSpan(slotStart),
                        EndTime = TimeOnly.FromTimeSpan(slotEnd),
                        IsAvailable = true
                    });
                }

                // Move to next slot (with optional buffer)
                slotStart = slotEnd.Add(TimeSpan.FromMinutes(DefaultBufferMinutes));
            }
        }

        return slots.OrderBy(s => s.StartTime).ToList();
    }

    /// <summary>
    /// Check if a specific date has any availability.
    /// </summary>
    public async Task<bool> HasAvailabilityAsync(Guid cardId, DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;

        // Check for whole-day block
        var isDayBlocked = await _db.AvailabilityExceptions
            .AnyAsync(e => e.CardId == cardId && 
                          e.ExceptionDate == date &&
                          e.ExceptionType == AvailabilityExceptionType.Blocked &&
                          e.StartTime == null);

        if (isDayBlocked) return false;

        // Check if there's a rule for this day
        var hasRule = await _db.AvailabilityRules
            .AnyAsync(r => r.CardId == cardId && r.DayOfWeek == dayOfWeek && r.IsActive);

        // Check for extra hours
        var hasExtraHours = await _db.AvailabilityExceptions
            .AnyAsync(e => e.CardId == cardId && 
                          e.ExceptionDate == date &&
                          e.ExceptionType == AvailabilityExceptionType.ExtraHours);

        return hasRule || hasExtraHours;
    }

    // ═══════════════════════════════════════════════════════════════
    // CRM API - Rules Management
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all availability rules for a card.
    /// </summary>
    public async Task<List<AvailabilityRule>> GetRulesAsync(Guid cardId)
    {
        return await _db.AvailabilityRules
            .Where(r => r.CardId == cardId)
            .OrderBy(r => r.DayOfWeek)
            .ToListAsync();
    }

    /// <summary>
    /// Save/update availability rules for a card.
    /// </summary>
    public async Task SaveRulesAsync(Guid cardId, List<AvailabilityRuleDto> rules)
    {
        // Remove existing rules
        var existing = await _db.AvailabilityRules
            .Where(r => r.CardId == cardId)
            .ToListAsync();
        _db.AvailabilityRules.RemoveRange(existing);

        // Add new rules
        foreach (var dto in rules)
        {
            _db.AvailabilityRules.Add(new AvailabilityRule
            {
                Id = Guid.NewGuid(),
                CardId = cardId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsActive = dto.IsActive
            });
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Create default availability rules (Mon-Fri 9-17).
    /// </summary>
    public async Task CreateDefaultRulesAsync(Guid cardId)
    {
        var rules = new List<AvailabilityRuleDto>();
        
        // Monday to Friday, 9:00 - 17:00
        for (int day = 1; day <= 5; day++)
        {
            rules.Add(new AvailabilityRuleDto
            {
                DayOfWeek = day,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                IsActive = true
            });
        }

        await SaveRulesAsync(cardId, rules);
    }

    // ═══════════════════════════════════════════════════════════════
    // CRM API - Exceptions Management
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get exceptions for a date range.
    /// </summary>
    public async Task<List<AvailabilityException>> GetExceptionsAsync(Guid cardId, DateOnly from, DateOnly to)
    {
        return await _db.AvailabilityExceptions
            .Where(e => e.CardId == cardId && 
                       e.ExceptionDate >= from && 
                       e.ExceptionDate <= to)
            .OrderBy(e => e.ExceptionDate)
            .ToListAsync();
    }

    /// <summary>
    /// Add/save an exception (holiday, blocked time, extra hours).
    /// </summary>
    public async Task<AvailabilityException> SaveExceptionAsync(AvailabilityException exception)
    {
        if (exception.Id == Guid.Empty)
        {
            exception.Id = Guid.NewGuid();
            _db.AvailabilityExceptions.Add(exception);
        }
        else
        {
            _db.AvailabilityExceptions.Update(exception);
        }

        await _db.SaveChangesAsync();
        return exception;
    }

    /// <summary>
    /// Delete an exception.
    /// </summary>
    public async Task DeleteExceptionAsync(Guid exceptionId)
    {
        var exception = await _db.AvailabilityExceptions.FindAsync(exceptionId);
        if (exception != null)
        {
            _db.AvailabilityExceptions.Remove(exception);
            await _db.SaveChangesAsync();
        }
    }
}

// ═══════════════════════════════════════════════════════════════
// DTOs
// ═══════════════════════════════════════════════════════════════

public class AvailabilityRuleDto
{
    public int DayOfWeek { get; set; } // 0=Sunday, 6=Saturday
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
