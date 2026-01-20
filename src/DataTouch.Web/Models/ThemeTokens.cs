namespace DataTouch.Web.Models;

/// <summary>
/// Complete theme tokens for consistent styling across preview and public card.
/// Each preset must define ALL these tokens for full theme coherence.
/// </summary>
public record ThemeTokens
{
    // ═══════════════════════════════════════════════════════════════
    // BACKGROUND
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Type of background: "gradient" or "solid"</summary>
    public string BgType { get; init; } = "gradient";
    
    /// <summary>CSS gradient or hex color value</summary>
    public string BgValue { get; init; } = "linear-gradient(135deg, #0F0A1E 0%, #1E1B4B 50%, #0F0A1E 100%)";
    
    /// <summary>Overlay opacity for legibility (0-1)</summary>
    public double BgOverlayOpacity { get; init; } = 0.3;
    
    /// <summary>Whether background is dark (for contrast calculation)</summary>
    public bool BgIsDark { get; init; } = true;
    
    // ═══════════════════════════════════════════════════════════════
    // SURFACES
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Card container background</summary>
    public string SurfaceCard { get; init; } = "rgba(255, 255, 255, 0.08)";
    
    /// <summary>Card border color</summary>
    public string SurfaceCardBorder { get; init; } = "rgba(255, 255, 255, 0.1)";
    
    /// <summary>Card style: "glass", "solid", or "elevated"</summary>
    public string SurfaceCardStyle { get; init; } = "glass";
    
    /// <summary>Input field background</summary>
    public string SurfaceInput { get; init; } = "rgba(0, 0, 0, 0.3)";
    
    /// <summary>Input field border (normal state)</summary>
    public string SurfaceInputBorder { get; init; } = "rgba(255, 255, 255, 0.1)";
    
    /// <summary>Input field border (focus state) - uses accent</summary>
    public string SurfaceInputFocus { get; init; } = "#7C3AED";
    
    /// <summary>Divider/separator color</summary>
    public string SurfaceDivider { get; init; } = "rgba(255, 255, 255, 0.1)";
    
    /// <summary>Neutral chip background</summary>
    public string SurfaceChip { get; init; } = "rgba(255, 255, 255, 0.1)";
    
    /// <summary>Neutral chip border</summary>
    public string SurfaceChipBorder { get; init; } = "rgba(255, 255, 255, 0.2)";
    
    // ═══════════════════════════════════════════════════════════════
    // TYPOGRAPHY
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Primary text color (names, titles)</summary>
    public string TextPrimary { get; init; } = "#FFFFFF";
    
    /// <summary>Secondary text color (subtitles, labels)</summary>
    public string TextSecondary { get; init; } = "rgba(255, 255, 255, 0.8)";
    
    /// <summary>Muted text color (bio, placeholders)</summary>
    public string TextMuted { get; init; } = "rgba(255, 255, 255, 0.6)";
    
    /// <summary>Text color on accent background (buttons)</summary>
    public string TextOnAccent { get; init; } = "#FFFFFF";
    
    // ═══════════════════════════════════════════════════════════════
    // ACCENT (THEME COLOR)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Primary accent: avatar ring, primary button, focus ring, highlights</summary>
    public string AccentPrimary { get; init; } = "#7C3AED";
    
    /// <summary>Soft accent for hover/pressed states</summary>
    public string AccentSoft { get; init; } = "rgba(124, 58, 237, 0.2)";
    
    /// <summary>Gradient for avatar ring</summary>
    public string AccentGradient { get; init; } = "linear-gradient(135deg, #7C3AED, #EC4899)";
    
    // ═══════════════════════════════════════════════════════════════
    // BUTTONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Primary button background (= accent primary)</summary>
    public string ButtonPrimaryBg { get; init; } = "#7C3AED";
    
    /// <summary>Primary button text (= text on accent)</summary>
    public string ButtonPrimaryText { get; init; } = "#FFFFFF";
    
    /// <summary>Primary button hover state</summary>
    public string ButtonPrimaryHover { get; init; } = "#6D28D9";
    
    /// <summary>Secondary button border</summary>
    public string ButtonSecondaryBorder { get; init; } = "rgba(255, 255, 255, 0.3)";
    
    /// <summary>Button shape: "pill", "rounded", or "square"</summary>
    public string ButtonShape { get; init; } = "pill";
    
    // ═══════════════════════════════════════════════════════════════
    // FOCUS/STATES
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>Focus ring color (with alpha)</summary>
    public string FocusRing { get; init; } = "rgba(124, 58, 237, 0.5)";
    
    /// <summary>Disabled state opacity</summary>
    public double StateDisabledOpacity { get; init; } = 0.5;
    
    /// <summary>Hover overlay</summary>
    public string StateHoverOverlay { get; init; } = "rgba(255, 255, 255, 0.05)";
    
    // ═══════════════════════════════════════════════════════════════
    // SEMANTIC (FIXED VALUES)
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>WhatsApp green (fixed)</summary>
    public string SemanticWhatsapp { get; init; } = "#25D366";
    
    /// <summary>Available status green (fixed)</summary>
    public string SemanticAvailable { get; init; } = "#22C55E";
    
    /// <summary>Email amber (fixed)</summary>
    public string SemanticEmail { get; init; } = "#F59E0B";
    
    /// <summary>Call purple (fixed)</summary>
    public string SemanticCall { get; init; } = "#8B5CF6";
}

/// <summary>
/// A complete preset definition with metadata and all tokens.
/// </summary>
public record ThemePreset
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; } // "dark" or "light"
    public required string Personality { get; init; }
    public required ThemeTokens Tokens { get; init; }
}
