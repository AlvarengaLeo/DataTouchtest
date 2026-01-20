namespace DataTouch.Web.Models;

/// <summary>
/// Configuration for quote form fields and auto-response.
/// Serialized to JSON in Service.QuoteFormConfigJson
/// </summary>
public class QuoteFormConfig
{
    /// <summary>Require client name (always true)</summary>
    public bool RequireName { get; set; } = true;
    
    /// <summary>Require WhatsApp number</summary>
    public bool RequireWhatsApp { get; set; } = true;
    
    /// <summary>Require email address</summary>
    public bool RequireEmail { get; set; } = true;
    
    /// <summary>Ask for company name</summary>
    public bool RequireCompany { get; set; } = false;
    
    /// <summary>Ask for estimated budget</summary>
    public bool RequireBudget { get; set; } = false;
    
    /// <summary>Ask for project details (textarea)</summary>
    public bool RequireDetails { get; set; } = true;
    
    /// <summary>Allow file attachment</summary>
    public bool AllowAttachment { get; set; } = false;
    
    /// <summary>Auto-response message sent to client</summary>
    public string AutoResponseMessage { get; set; } = "Gracias por tu solicitud. Te respondo en 24 horas.";
}
