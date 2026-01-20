namespace DataTouch.Domain.Entities;

/// <summary>
/// Appointment/booking created by visitors from public card.
/// </summary>
public class Appointment
{
    public Guid Id { get; set; }
    
    /// <summary>Card this appointment belongs to</summary>
    public Guid CardId { get; set; }
    
    /// <summary>Organization for multi-tenant queries</summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>Optional: Service being booked</summary>
    public Guid? ServiceId { get; set; }
    
    /// <summary>Appointment start (UTC)</summary>
    public DateTime StartDateTime { get; set; }
    
    /// <summary>Appointment end (UTC)</summary>
    public DateTime EndDateTime { get; set; }
    
    /// <summary>Timezone of the card owner (e.g., "America/El_Salvador")</summary>
    public string Timezone { get; set; } = "America/El_Salvador";
    
    /// <summary>Pending, Confirmed, Completed, Cancelled, NoShow</summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    
    // Customer info (visitor who booked)
    public string CustomerName { get; set; } = default!;
    public string CustomerEmail { get; set; } = default!;
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public string? CustomerNotes { get; set; }
    
    /// <summary>Internal notes (CRM use only)</summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>Source: PublicCard, Admin, Quote</summary>
    public string Source { get; set; } = "PublicCard";
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Cancel tracking (for undo/restore)
    public DateTime? CancelledAt { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancelReason { get; set; }
    
    /// <summary>Stores previous status before cancel for restore</summary>
    public AppointmentStatus? PreviousStatus { get; set; }
    
    // Navigation
    public Card Card { get; set; } = default!;
    public Organization Organization { get; set; } = default!;
    public Service? Service { get; set; }
}

/// <summary>
/// Appointment lifecycle status
/// </summary>
public enum AppointmentStatus
{
    Pending = 0,
    Confirmed = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}
