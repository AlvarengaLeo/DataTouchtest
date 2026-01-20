namespace DataTouch.Domain.Entities;

/// <summary>
/// Exception to normal availability (blocked days, extra hours).
/// </summary>
public class AvailabilityException
{
    public Guid Id { get; set; }
    
    /// <summary>Card this exception belongs to</summary>
    public Guid CardId { get; set; }
    
    /// <summary>Specific date of the exception</summary>
    public DateOnly ExceptionDate { get; set; }
    
    /// <summary>Start time (null if whole day blocked)</summary>
    public TimeSpan? StartTime { get; set; }
    
    /// <summary>End time (null if whole day blocked)</summary>
    public TimeSpan? EndTime { get; set; }
    
    /// <summary>Type: Blocked (no availability) or ExtraHours (additional time)</summary>
    public AvailabilityExceptionType ExceptionType { get; set; } = AvailabilityExceptionType.Blocked;
    
    // Navigation
    public Card Card { get; set; } = default!;
}

/// <summary>
/// Type of availability exception
/// </summary>
public enum AvailabilityExceptionType
{
    /// <summary>Block this date/time (no bookings)</summary>
    Blocked = 0,
    
    /// <summary>Add extra hours on this date</summary>
    ExtraHours = 1
}
