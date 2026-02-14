namespace DataTouch.Web.Models;

/// <summary>
/// Shared model for card appearance/style, serialized to Card.AppearanceStyleJson.
/// Single source of truth — used by both MyCard.razor and PublicCard.razor.
/// </summary>
public class CardStyleModel
{
    // Fondo Global
    public string BackgroundType { get; set; } = "gradient"; // gradient, solid, image
    public string BackgroundValue { get; set; } = "linear-gradient(135deg, #0F0A1E 0%, #1E1B4B 50%, #0F0A1E 100%)";
    public string? BackgroundImageUrl { get; set; }
    
    // Tarjeta - Vinculación e Independencia
    public bool CardLinkedToBackground { get; set; } = true;
    public string CardContainerStyle { get; set; } = "glass"; // glass, solid, elevated
    public string CardBackgroundColor { get; set; } = "#1A1035";
    public int GlassIntensity { get; set; } = 50; // 0-100
    
    // Avanzado - Fondo
    public double OverlayOpacity { get; set; } = 0.5;
    public bool ShowGrain { get; set; } = false;
    
    // Avanzado - Contenedor
    public string BorderRadius { get; set; } = "medium"; // small, medium, large
    public string ShadowStyle { get; set; } = "soft"; // soft, medium, strong
    
    // Avanzado - Avatar
    public string AvatarSize { get; set; } = "M"; // S, M, L
    public bool AvatarGlow { get; set; } = true;
    
    // Avanzado - Botones
    public string ButtonShape { get; set; } = "pill"; // pill, rounded, square
    public string ButtonStyle { get; set; } = "filled"; // filled, outline
    
    // Contraste automático
    public string ContrastMode { get; set; } = "auto"; // auto, force-light, force-dark
    public bool BackgroundIsDark { get; set; } = true;
    public bool CardIsDark { get; set; } = true;
    public bool CardIsDarkOverride { get; set; } = true;
    
    // Preset tracking
    public string? PresetId { get; set; } = "premium-dark";
    public string? AccentColor { get; set; } = "#7C3AED";
}
