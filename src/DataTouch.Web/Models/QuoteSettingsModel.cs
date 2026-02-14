namespace DataTouch.Web.Models;

/// <summary>
/// Settings for the QuoteRequest template.
/// Serialized to Card.QuoteSettingsJson.
/// Controls the public quote request block copy and form field visibility.
/// </summary>
public class QuoteSettingsModel
{
    // ── Copy del bloque ──
    public string BlockTitle { get; set; } = "Solicitar cotización";
    public string BlockSubtitle { get; set; } = "Describe lo que necesitas y te respondo pronto.";
    public string? LegalText { get; set; }
    public bool ShowLegalText { get; set; } = false;

    // ── Configuración de campos del formulario ──
    public bool EmailRequired { get; set; } = true;
    public bool PhoneRequired { get; set; } = false;
    public bool ShowBudgetField { get; set; } = true;
    public bool ShowDeadlineField { get; set; } = true;
    public bool ShowPreferredContactField { get; set; } = true;

    // ── Confirmación ──
    public string SuccessMessage { get; set; } = "Solicitud enviada. Te responderé pronto.";
}
