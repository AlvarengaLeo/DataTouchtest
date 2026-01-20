namespace DataTouch.Domain.Entities;

public class Lead
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid CardId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? PhoneCountryCode { get; set; }  // e.g. "+503"
    public string? PhoneE164 { get; set; }         // e.g. "+50370000000" full E.164 format
    public string? Message { get; set; }
    public string Source { get; set; } = "CARD_CONTACT_FORM";
    public string Status { get; set; } = "New";
    public DateTime CreatedAt { get; set; }
    public string? InternalNotes { get; set; }
    public DateTime? NotesUpdatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }

    public Organization Organization { get; set; } = default!;
    public Card Card { get; set; } = default!;
    public User OwnerUser { get; set; } = default!;
    public ICollection<LeadNote> Notes { get; set; } = new List<LeadNote>();
}
