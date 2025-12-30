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

    
    // ═══════════════════════════════════════════════════════════════
    // TEMPLATE & STYLE (NEW - QRCodeChimp-style)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Plantilla base usada para esta tarjeta (opcional)</summary>
    public Guid? TemplateId { get; set; }
    
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
}

