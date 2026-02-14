namespace DataTouch.Domain.Entities;

/// <summary>
/// Services offered by a card owner for booking/quoting.
/// </summary>
public class Service
{
    public Guid Id { get; set; }
    
    /// <summary>Card this service belongs to</summary>
    public Guid CardId { get; set; }
    
    /// <summary>Organization for multi-tenant queries</summary>
    public Guid OrganizationId { get; set; }
    
    /// <summary>Service name (e.g., "Consultoría empresarial")</summary>
    public string Name { get; set; } = default!;
    
    /// <summary>Short description of the service</summary>
    public string? Description { get; set; }
    
    /// <summary>Duration in minutes (default 30)</summary>
    public int DurationMinutes { get; set; } = 30;
    
    /// <summary>Starting price (displayed as "desde $X")</summary>
    public decimal? PriceFrom { get; set; }
    
    /// <summary>Optional category for grouping</summary>
    public string? CategoryName { get; set; }
    
    /// <summary>Display order in the list</summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>Whether service is available for booking</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Conversion type: "booking", "quote", or "both"</summary>
    public string ConversionType { get; set; } = "booking";
    
    /// <summary>Service modality: "presencial", "online", "domicilio"</summary>
    public string? Modality { get; set; }
    
    /// <summary>Buffer before appointment in minutes (overrides global)</summary>
    public int? BufferBeforeMinutes { get; set; }
    
    /// <summary>Buffer after appointment in minutes (overrides global)</summary>
    public int? BufferAfterMinutes { get; set; }
    
    /// <summary>Min notice for this service in minutes (overrides global)</summary>
    public int? MinNoticeMinutes { get; set; }
    
    /// <summary>Max bookings per day for this service (0=unlimited)</summary>
    public int? MaxBookingsPerDay { get; set; }
    
    /// <summary>Whether this service uses the card's global schedule (true) or has its own override (false)</summary>
    public bool UseGlobalSchedule { get; set; } = true;
    
    /// <summary>Quote form config JSON (fields enabled, auto-response)</summary>
    public string? QuoteFormConfigJson { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Card Card { get; set; } = default!;
    public Organization Organization { get; set; } = default!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
