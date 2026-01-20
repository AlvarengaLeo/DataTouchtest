namespace DataTouch.Domain.Entities;

/// <summary>
/// Booking configuration settings for a card (slot intervals, buffers, limits).
/// </summary>
public class BookingSettings
{
    public Guid Id { get; set; }
    
    /// <summary>Card this settings belongs to</summary>
    public Guid CardId { get; set; }
    
    /// <summary>Timezone ID (IANA format, e.g., "America/El_Salvador")</summary>
    public string TimeZoneId { get; set; } = "America/El_Salvador";
    
    /// <summary>Slot interval in minutes (default 30)</summary>
    public int SlotIntervalMinutes { get; set; } = 30;
    
    /// <summary>Buffer time before appointment in minutes</summary>
    public int BufferBeforeMinutes { get; set; } = 0;
    
    /// <summary>Buffer time after appointment in minutes</summary>
    public int BufferAfterMinutes { get; set; } = 0;
    
    /// <summary>Maximum appointments per day (0 = unlimited)</summary>
    public int MaxAppointmentsPerDay { get; set; } = 0;
    
    /// <summary>Minimum notice time for booking in minutes (0 = immediate)</summary>
    public int MinNoticeMinutes { get; set; } = 0;
    
    /// <summary>How many days in advance can clients book (0 = unlimited)</summary>
    public int MaxAdvanceDays { get; set; } = 60;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Card Card { get; set; } = default!;
}
