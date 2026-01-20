namespace DataTouch.Domain.Entities;

/// <summary>
/// Timeline event / activity for CRM entities (Lead, QuoteRequest, Appointment).
/// Provides full audit trail and history.
/// </summary>
public class Activity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    
    // Polymorphic relation
    public string EntityType { get; set; } = default!; // "Lead", "QuoteRequest", "Appointment"
    public Guid EntityId { get; set; }
    
    // Activity data
    public ActivityType Type { get; set; }
    public string Description { get; set; } = default!;
    public string? MetadataJson { get; set; } // {old_status, new_status, note, etc.}
    
    // Actor (null = system action)
    public Guid? UserId { get; set; }
    public string? SystemSource { get; set; } // "automation", "api", "webhook"
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation
    public Organization Organization { get; set; } = default!;
    public User? User { get; set; }
}

/// <summary>
/// Types of activities that can be logged
/// </summary>
public enum ActivityType
{
    /// <summary>Entity was created</summary>
    Created = 0,
    
    /// <summary>Status changed</summary>
    StatusChange = 1,
    
    /// <summary>Note added</summary>
    Note = 2,
    
    /// <summary>Email sent</summary>
    EmailSent = 3,
    
    /// <summary>Email received</summary>
    EmailReceived = 4,
    
    /// <summary>Call made</summary>
    Call = 5,
    
    /// <summary>Owner assigned/changed</summary>
    Assignment = 6,
    
    /// <summary>Converted (quote to appointment, etc)</summary>
    Conversion = 7,
    
    /// <summary>Attachment added</summary>
    AttachmentAdded = 8,
    
    /// <summary>SLA warning/escalation</summary>
    SlaAlert = 9,
    
    /// <summary>Data merged from duplicate</summary>
    Merge = 10,
    
    /// <summary>System automation triggered</summary>
    Automation = 11
}
