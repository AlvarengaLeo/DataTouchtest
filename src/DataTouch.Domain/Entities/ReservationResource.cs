namespace DataTouch.Domain.Entities;

/// <summary>
/// Bookable resource for reservations template (cabaña, casa, salón, habitación, etc.).
/// If a card has only 1 resource, the selector is hidden in the UI.
/// </summary>
public class ReservationResource
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int MaxGuests { get; set; } = 10;
    public decimal? PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    // Blocked dates (JSON array of date strings, e.g. ["2026-03-15","2026-03-16"])
    public string? BlockedDatesJson { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    public Card Card { get; set; } = default!;
    public ICollection<ReservationRequest> ReservationRequests { get; set; } = new List<ReservationRequest>();
}
