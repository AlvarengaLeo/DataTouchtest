namespace DataTouch.Web.Models;

/// <summary>
/// Helper class for generating CSS custom properties from ThemeTokens.
/// Used by both Live Preview and Public Card to ensure consistency.
/// </summary>
public static class ThemeHelper
{
    /// <summary>
    /// Generates CSS custom properties from ThemeTokens.
    /// Returns a style string to be applied to the root container.
    /// </summary>
    public static string GenerateCssVariables(ThemeTokens tokens)
    {
        return $@"
            --dt-bg-type: {tokens.BgType};
            --dt-bg-value: {tokens.BgValue};
            --dt-bg-overlay-opacity: {tokens.BgOverlayOpacity.ToString(System.Globalization.CultureInfo.InvariantCulture)};
            
            --dt-surface-card: {tokens.SurfaceCard};
            --dt-surface-card-border: {tokens.SurfaceCardBorder};
            --dt-surface-card-style: {tokens.SurfaceCardStyle};
            --dt-surface-input: {tokens.SurfaceInput};
            --dt-surface-input-border: {tokens.SurfaceInputBorder};
            --dt-surface-input-focus: {tokens.SurfaceInputFocus};
            --dt-surface-divider: {tokens.SurfaceDivider};
            --dt-surface-chip: {tokens.SurfaceChip};
            --dt-surface-chip-border: {tokens.SurfaceChipBorder};
            
            --dt-text-primary: {tokens.TextPrimary};
            --dt-text-secondary: {tokens.TextSecondary};
            --dt-text-muted: {tokens.TextMuted};
            --dt-text-on-accent: {tokens.TextOnAccent};
            
            --dt-accent-primary: {tokens.AccentPrimary};
            --dt-accent-soft: {tokens.AccentSoft};
            --dt-accent-gradient: {tokens.AccentGradient};
            
            --dt-button-primary-bg: {tokens.ButtonPrimaryBg};
            --dt-button-primary-text: {tokens.ButtonPrimaryText};
            --dt-button-primary-hover: {tokens.ButtonPrimaryHover};
            --dt-button-secondary-border: {tokens.ButtonSecondaryBorder};
            --dt-button-shape: {GetBorderRadius(tokens.ButtonShape)};
            
            --dt-focus-ring: {tokens.FocusRing};
            --dt-state-disabled-opacity: {tokens.StateDisabledOpacity.ToString(System.Globalization.CultureInfo.InvariantCulture)};
            --dt-state-hover-overlay: {tokens.StateHoverOverlay};
            
            --dt-semantic-whatsapp: {tokens.SemanticWhatsapp};
            --dt-semantic-available: {tokens.SemanticAvailable};
            --dt-semantic-email: {tokens.SemanticEmail};
            --dt-semantic-call: {tokens.SemanticCall};
        ".Trim();
    }
    
    /// <summary>
    /// Generates the background style for the wrapper element.
    /// </summary>
    public static string GetBackgroundStyle(ThemeTokens tokens)
    {
        var bgStyle = tokens.BgType == "gradient" 
            ? $"background: {tokens.BgValue};" 
            : $"background: {tokens.BgValue};";
        
        return bgStyle;
    }
    
    /// <summary>
    /// Gets the CSS blur value for glass effect based on card style.
    /// </summary>
    public static string GetGlassBlur(ThemeTokens tokens, int intensity = 50)
    {
        if (tokens.SurfaceCardStyle != "glass") 
            return "backdrop-filter: none;";
        
        var blurPx = Math.Max(4, intensity / 10); // 4-10px based on intensity
        return $"backdrop-filter: blur({blurPx}px);";
    }
    
    /// <summary>
    /// Converts button shape to border-radius value.
    /// </summary>
    private static string GetBorderRadius(string shape) => shape switch
    {
        "pill" => "9999px",
        "square" => "4px",
        _ => "12px" // rounded (default)
    };
    
    /// <summary>
    /// Gets the appropriate card shadow based on style.
    /// </summary>
    public static string GetCardShadow(ThemeTokens tokens)
    {
        return tokens.SurfaceCardStyle switch
        {
            "elevated" => "0 8px 30px rgba(0, 0, 0, 0.15), 0 4px 12px rgba(0, 0, 0, 0.1)",
            "solid" => "0 2px 8px rgba(0, 0, 0, 0.1)",
            _ => "0 4px 20px rgba(0, 0, 0, 0.15)" // glass
        };
    }
    
    /// <summary>
    /// Creates a ThemeTokens instance from legacy CardStyleModel settings.
    /// Used for migration and backwards compatibility.
    /// </summary>
    public static ThemeTokens CreateFromLegacy(string? presetId, string? accentColor, bool bgIsDark)
    {
        // Try to find the preset
        var preset = PresetRegistry.GetById(presetId ?? "premium-dark") ?? PresetRegistry.Default;
        var baseTokens = preset.Tokens;
        
        // If a custom accent was specified, override the accent tokens
        if (!string.IsNullOrEmpty(accentColor) && accentColor != baseTokens.AccentPrimary)
        {
            return baseTokens with
            {
                AccentPrimary = accentColor,
                AccentSoft = $"rgba({HexToRgb(accentColor)}, 0.2)",
                AccentGradient = $"linear-gradient(135deg, {accentColor}, {DarkenHex(accentColor, 0.2)})",
                ButtonPrimaryBg = accentColor,
                ButtonPrimaryHover = DarkenHex(accentColor, 0.15),
                SurfaceInputFocus = accentColor,
                FocusRing = $"rgba({HexToRgb(accentColor)}, 0.5)"
            };
        }
        
        return baseTokens;
    }
    
    /// <summary>
    /// Converts hex color to RGB components string.
    /// </summary>
    private static string HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6) return "124, 58, 237"; // fallback
        
        try
        {
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return $"{r}, {g}, {b}";
        }
        catch
        {
            return "124, 58, 237"; // fallback violet
        }
    }
    
    /// <summary>
    /// Darkens a hex color by a percentage.
    /// </summary>
    private static string DarkenHex(string hex, double amount)
    {
        hex = hex.TrimStart('#');
        if (hex.Length < 6) return "#6D28D9"; // fallback
        
        try
        {
            int r = Math.Max(0, (int)(Convert.ToInt32(hex.Substring(0, 2), 16) * (1 - amount)));
            int g = Math.Max(0, (int)(Convert.ToInt32(hex.Substring(2, 2), 16) * (1 - amount)));
            int b = Math.Max(0, (int)(Convert.ToInt32(hex.Substring(4, 2), 16) * (1 - amount)));
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch
        {
            return "#6D28D9"; // fallback
        }
    }
}
