namespace DataTouch.Domain.Entities;

/// <summary>
/// Quote/cotización request from public card visitors.
/// Enterprise-grade with full state machine, SLA tracking, and audit support.
/// </summary>
public class QuoteRequest
{
    public Guid Id { get; set; }
    
    // Organization & Card
    public Guid OrganizationId { get; set; }
    public Guid CardId { get; set; }
    
    // Service requested (nullable for quote-request template which has no service catalog)
    public Guid? ServiceId { get; set; }
    
    // Lead reference (for CRM integration)
    public Guid? LeadId { get; set; }
    
    // Reference number (human-readable)
    public string RequestNumber { get; set; } = default!; // "QR-2026-0042"
    
    // Customer info (denormalized for quick access)
    public string CustomerName { get; set; } = default!;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneCountryCode { get; set; }
    public string? CustomerCompany { get; set; }
    
    // Request details
    public string? Description { get; set; }
    public string? AttachmentsJson { get; set; } // [{name, url, size}]
    public string? CustomFieldsJson { get; set; } // Dynamic service fields
    
    // State machine (8 states per enterprise spec)
    public QuoteStatus Status { get; set; } = QuoteStatus.New;
    public string? StatusReason { get; set; } // For lost/archived
    
    // Assignment
    public Guid? OwnerId { get; set; }
    public int Priority { get; set; } = 2; // 1=Low, 2=Med, 3=High
    
    // SLA tracking
    public DateTime? FirstResponseAt { get; set; }
    public DateTime? LastContactAt { get; set; }
    public DateTime? SlaDeadlineAt { get; set; }
    public bool SlaNotified { get; set; } = false;
    
    // Outcome
    public decimal? QuotedAmount { get; set; }
    public decimal? FinalAmount { get; set; }
    public DateTime? WonAt { get; set; }
    public DateTime? LostAt { get; set; }
    
    // Idempotency (prevent duplicate submissions)
    public string? IdempotencyKey { get; set; }
    
    // Source tracking
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
    
    // Internal notes (CRM use only)
    public string? InternalNotes { get; set; }
    
    // Conversion to appointment
    public Guid? ConvertedAppointmentId { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = default!;
    public Card Card { get; set; } = default!;
    public Service? Service { get; set; }
    public User? Owner { get; set; }
    public Appointment? ConvertedAppointment { get; set; }
}

/// <summary>
/// Quote request lifecycle status (enterprise 8-state machine)
/// </summary>
public enum QuoteStatus
{
    /// <summary>Recién recibida, sin revisar</summary>
    New = 0,
    
    /// <summary>Owner la está analizando</summary>
    InReview = 1,
    
    /// <summary>Se pidió más info al cliente</summary>
    NeedsInfo = 2,
    
    /// <summary>Cotización enviada al cliente</summary>
    Quoted = 3,
    
    /// <summary>En proceso de negociación</summary>
    Negotiation = 4,
    
    /// <summary>Cliente aceptó, venta cerrada</summary>
    Won = 5,
    
    /// <summary>Cliente rechazó o no respondió</summary>
    Lost = 6,
    
    /// <summary>Movida a histórico</summary>
    Archived = 7
}
