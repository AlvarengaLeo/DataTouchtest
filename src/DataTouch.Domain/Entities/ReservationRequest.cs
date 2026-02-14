namespace DataTouch.Domain.Entities;

/// <summary>
/// Reservation request from public card visitors (hotel-style date range booking).
/// Template: reservations-range. CTA: "RESERVAR".
/// </summary>
public class ReservationRequest
{
    public Guid Id { get; set; }
    
    // Organization & Card
    public Guid OrganizationId { get; set; }
    public Guid CardId { get; set; }
    
    // Resource (optional — cabaña/casa/salón)
    public Guid? ResourceId { get; set; }
    
    // Lead reference (for CRM integration)
    public Guid? LeadId { get; set; }
    
    // Reference number (human-readable)
    public string RequestNumber { get; set; } = default!; // "RR-2026-0001"
    
    // Date range
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Nights { get; set; }
    
    // Guests
    public int GuestsAdults { get; set; } = 1;
    public int GuestsChildren { get; set; } = 0;
    
    // Extras & notes
    public string? ExtrasJson { get; set; } // ["Late check-out", "Desayuno"]
    public string? Notes { get; set; }
    
    // Customer info (denormalized for quick access)
    public string ContactName { get; set; } = default!;
    public string? ContactPhone { get; set; }
    public string? ContactPhoneCountryCode { get; set; }
    public string? ContactEmail { get; set; }
    
    // State machine (4 states)
    public ReservationStatus Status { get; set; } = ReservationStatus.New;
    public string? StatusReason { get; set; }
    
    // Source tracking
    public string Source { get; set; } = "PublicCard";
    public string TemplateKey { get; set; } = "ReservationsRange";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Idempotency
    public string? IdempotencyKey { get; set; }
    
    // Internal notes (CRM use only)
    public string? InternalNotes { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = default!;
    public Card Card { get; set; } = default!;
    public ReservationResource? Resource { get; set; }
}

/// <summary>
/// Reservation request lifecycle status
/// </summary>
public enum ReservationStatus
{
    /// <summary>Recién recibida, sin revisar</summary>
    New = 0,
    
    /// <summary>En revisión por el propietario</summary>
    InReview = 1,
    
    /// <summary>Confirmada</summary>
    Confirmed = 2,
    
    /// <summary>Cancelada</summary>
    Cancelled = 3
}
