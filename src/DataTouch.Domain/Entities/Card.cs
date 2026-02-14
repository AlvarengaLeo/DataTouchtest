namespace DataTouch.Domain.Entities;

public class Card
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Slug { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Title { get; set; }
    public string? CompanyName { get; set; }
    public string? Bio { get; set; }
    public string? Phone { get; set; }
    public string? PhoneCountryCode { get; set; } // e.g. "+52"
    public string? WhatsAppNumber { get; set; }
    public string? WhatsAppCountryCode { get; set; } // e.g. "+52"
    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }
    
    // CTA Visibility Flags
    public bool ShowSaveContact { get; set; } = true;
    public bool ShowWhatsApp { get; set; } = true;
    public bool ShowCall { get; set; } = true;
    public bool ShowEmail { get; set; } = true;
    
    // JSON Storage for Links
    public string? SocialLinksJson { get; set; }  // {"linkedin": "url", "instagram": "url", ...}
    public string? WebsiteLinksJson { get; set; }  // [{"title": "text", "url": "url"}, ...]
    
    // JSON Storage for Portfolio Gallery
    /// <summary>
    /// Gallery images for portfolio template: [{url, title, description, order}, ...]
    /// </summary>
    public string? GalleryImagesJson { get; set; }

    /// <summary>
    /// Portfolio gallery with photos/videos sections and enable flags.
    /// Supersedes GalleryImagesJson (kept for backward compat migration).
    /// </summary>
    public string? PortfolioGalleryJson { get; set; }
    
    /// <summary>
    /// JSON storage for appearance customization (background, card style, buttons, etc.)
    /// </summary>
    public string? AppearanceStyleJson { get; set; }

    /// <summary>
    /// JSON storage for quote request template settings (title, subtitle, form config, etc.)
    /// </summary>
    public string? QuoteSettingsJson { get; set; }

    /// <summary>
    /// JSON storage for reservation template settings (min nights, max guests, extras, policies, etc.)
    /// </summary>
    public string? ReservationSettingsJson { get; set; }

    
    // ═══════════════════════════════════════════════════════════════
    // TEMPLATE & STYLE (NEW - QRCodeChimp-style)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Plantilla base usada para esta tarjeta (opcional)</summary>
    public Guid? TemplateId { get; set; }
    
    /// <summary>Tipo de plantilla: 'default', 'portfolio-creative', 'services-quotes', etc.</summary>
    public string TemplateType { get; set; } = "default";
    
    /// <summary>Primary card goal: "booking" or "quote" - defines which CTA is primary</summary>
    public string PrimaryCardGoal { get; set; } = "booking";
    
    /// <summary>Estilo personalizado de esta tarjeta (opcional)</summary>
    public Guid? StyleId { get; set; }
    
    // ═══════════════════════════════════════════════════════════════
    // SECURITY (NEW)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Hash de contraseña para proteger la tarjeta (opcional)</summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>Fecha desde la cual la tarjeta está activa</summary>
    public DateTime? ActiveFrom { get; set; }
    
    /// <summary>Fecha hasta la cual la tarjeta está activa</summary>
    public DateTime? ActiveUntil { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Organization Organization { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    
    // NEW Navigation Properties
    public CardTemplate? Template { get; set; }
    public CardStyle? Style { get; set; }
    public ICollection<CardComponent> Components { get; set; } = new List<CardComponent>();
    public ICollection<CardAnalytics> Analytics { get; set; } = new List<CardAnalytics>();
    
    // ═══════════════════════════════════════════════════════════════
    // BOOKING SYSTEM Navigation Properties
    // ═══════════════════════════════════════════════════════════════
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<AvailabilityRule> AvailabilityRules { get; set; } = new List<AvailabilityRule>();
    public ICollection<AvailabilityException> AvailabilityExceptions { get; set; } = new List<AvailabilityException>();
    public ICollection<QuoteRequest> QuoteRequests { get; set; } = new List<QuoteRequest>();
    public ICollection<ReservationRequest> ReservationRequests { get; set; } = new List<ReservationRequest>();
    public ICollection<ReservationResource> ReservationResources { get; set; } = new List<ReservationResource>();
}

