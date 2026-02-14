namespace DataTouch.Domain.Entities;

/// <summary>
/// Weekly availability rule for a card (recurring schedule).
/// </summary>
public class AvailabilityRule
{
    public Guid Id { get; set; }
    
    /// <summary>Card this rule belongs to</summary>
    public Guid CardId { get; set; }
    
    /// <summary>Day of week: 0=Sunday, 1=Monday, ..., 6=Saturday</summary>
    public int DayOfWeek { get; set; }
    
    /// <summary>Start time of availability window</summary>
    public TimeSpan StartTime { get; set; }
    
    /// <summary>End time of availability window</summary>
    public TimeSpan EndTime { get; set; }
    
    /// <summary>Break start time (optional lunch/rest)</summary>
    public TimeSpan? BreakStartTime { get; set; }
    
    /// <summary>Break end time (optional lunch/rest)</summary>
    public TimeSpan? BreakEndTime { get; set; }
    
    /// <summary>Service-specific override (null = global rule)</summary>
    public Guid? ServiceId { get; set; }
    
    /// <summary>Whether this day is active for bookings</summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public Card Card { get; set; } = default!;
    public Service? Service { get; set; }
}
