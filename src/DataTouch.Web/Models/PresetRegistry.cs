namespace DataTouch.Web.Models;

/// <summary>
/// Registry of all available theme presets.
/// Single source of truth for theme definitions.
/// </summary>
public static class PresetRegistry
{
    /// <summary>
    /// All available presets (8 dark + 4 light)
    /// </summary>
    public static IReadOnlyList<ThemePreset> All { get; } = new List<ThemePreset>
    {
        // ═══════════════════════════════════════════════════════════════
        // DARK PRESETS (8)
        // ═══════════════════════════════════════════════════════════════
        
        new ThemePreset
        {
            Id = "premium-dark",
            Name = "Premium Dark",
            Category = "dark",
            Personality = "Ejecutivos, consultores",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #0F0A1E 0%, #1E1B4B 50%, #0F0A1E 100%)",
                BgOverlayOpacity = 0.3,
                BgIsDark = true,
                SurfaceCard = "rgba(255, 255, 255, 0.08)",
                SurfaceCardBorder = "rgba(255, 255, 255, 0.1)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.3)",
                SurfaceInputBorder = "rgba(255, 255, 255, 0.1)",
                SurfaceInputFocus = "#7C3AED",
                SurfaceDivider = "rgba(255, 255, 255, 0.1)",
                SurfaceChip = "rgba(255, 255, 255, 0.1)",
                SurfaceChipBorder = "rgba(255, 255, 255, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.8)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#7C3AED",
                AccentSoft = "rgba(124, 58, 237, 0.2)",
                AccentGradient = "linear-gradient(135deg, #7C3AED, #EC4899)",
                ButtonPrimaryBg = "#7C3AED",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#6D28D9",
                ButtonSecondaryBorder = "rgba(255, 255, 255, 0.3)",
                ButtonShape = "pill",
                FocusRing = "rgba(124, 58, 237, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "midnight-blue",
            Name = "Midnight Blue",
            Category = "dark",
            Personality = "Tech, startups",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #0c1445 0%, #1a237e 50%, #0d1b3e 100%)",
                BgOverlayOpacity = 0.25,
                BgIsDark = true,
                SurfaceCard = "rgba(255, 255, 255, 0.06)",
                SurfaceCardBorder = "rgba(0, 180, 216, 0.2)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.35)",
                SurfaceInputBorder = "rgba(0, 180, 216, 0.15)",
                SurfaceInputFocus = "#00B4D8",
                SurfaceDivider = "rgba(0, 180, 216, 0.1)",
                SurfaceChip = "rgba(0, 180, 216, 0.1)",
                SurfaceChipBorder = "rgba(0, 180, 216, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.85)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#00B4D8",
                AccentSoft = "rgba(0, 180, 216, 0.2)",
                AccentGradient = "linear-gradient(135deg, #00B4D8, #0077B6)",
                ButtonPrimaryBg = "#00B4D8",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#0096C7",
                ButtonSecondaryBorder = "rgba(0, 180, 216, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(0, 180, 216, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "charcoal-gold",
            Name = "Charcoal Gold",
            Category = "dark",
            Personality = "Lujo, finanzas",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#1C1C1C",
                BgOverlayOpacity = 0,
                BgIsDark = true,
                SurfaceCard = "rgba(212, 175, 55, 0.05)",
                SurfaceCardBorder = "rgba(212, 175, 55, 0.2)",
                SurfaceCardStyle = "solid",
                SurfaceInput = "rgba(0, 0, 0, 0.4)",
                SurfaceInputBorder = "rgba(212, 175, 55, 0.15)",
                SurfaceInputFocus = "#D4AF37",
                SurfaceDivider = "rgba(212, 175, 55, 0.1)",
                SurfaceChip = "rgba(212, 175, 55, 0.1)",
                SurfaceChipBorder = "rgba(212, 175, 55, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.85)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#1C1C1C",
                AccentPrimary = "#D4AF37",
                AccentSoft = "rgba(212, 175, 55, 0.2)",
                AccentGradient = "linear-gradient(135deg, #D4AF37, #B8860B)",
                ButtonPrimaryBg = "#D4AF37",
                ButtonPrimaryText = "#1C1C1C",
                ButtonPrimaryHover = "#C9A227",
                ButtonSecondaryBorder = "rgba(212, 175, 55, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(212, 175, 55, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "emerald-night",
            Name = "Emerald Night",
            Category = "dark",
            Personality = "Eco, wellness",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #0D1F17 0%, #1A3D2E 50%, #0D1F17 100%)",
                BgOverlayOpacity = 0.2,
                BgIsDark = true,
                SurfaceCard = "rgba(16, 185, 129, 0.06)",
                SurfaceCardBorder = "rgba(16, 185, 129, 0.15)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.35)",
                SurfaceInputBorder = "rgba(16, 185, 129, 0.15)",
                SurfaceInputFocus = "#10B981",
                SurfaceDivider = "rgba(16, 185, 129, 0.1)",
                SurfaceChip = "rgba(16, 185, 129, 0.1)",
                SurfaceChipBorder = "rgba(16, 185, 129, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.85)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#10B981",
                AccentSoft = "rgba(16, 185, 129, 0.2)",
                AccentGradient = "linear-gradient(135deg, #10B981, #059669)",
                ButtonPrimaryBg = "#10B981",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#059669",
                ButtonSecondaryBorder = "rgba(16, 185, 129, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(16, 185, 129, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "rose-noir",
            Name = "Rose Noir",
            Category = "dark",
            Personality = "Creative, beauty",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #1A0A14 0%, #2D1A24 50%, #1A0A14 100%)",
                BgOverlayOpacity = 0.2,
                BgIsDark = true,
                SurfaceCard = "rgba(244, 114, 182, 0.06)",
                SurfaceCardBorder = "rgba(244, 114, 182, 0.15)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.35)",
                SurfaceInputBorder = "rgba(244, 114, 182, 0.15)",
                SurfaceInputFocus = "#F472B6",
                SurfaceDivider = "rgba(244, 114, 182, 0.1)",
                SurfaceChip = "rgba(244, 114, 182, 0.1)",
                SurfaceChipBorder = "rgba(244, 114, 182, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.85)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#F472B6",
                AccentSoft = "rgba(244, 114, 182, 0.2)",
                AccentGradient = "linear-gradient(135deg, #F472B6, #EC4899)",
                ButtonPrimaryBg = "#F472B6",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#EC4899",
                ButtonSecondaryBorder = "rgba(244, 114, 182, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(244, 114, 182, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "high-contrast",
            Name = "High Contrast",
            Category = "dark",
            Personality = "Accesibilidad",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#000000",
                BgOverlayOpacity = 0,
                BgIsDark = true,
                SurfaceCard = "#0A0A0A",
                SurfaceCardBorder = "#FFFFFF",
                SurfaceCardStyle = "solid",
                SurfaceInput = "#000000",
                SurfaceInputBorder = "#FFFFFF",
                SurfaceInputFocus = "#FFFFFF",
                SurfaceDivider = "#FFFFFF",
                SurfaceChip = "#000000",
                SurfaceChipBorder = "#FFFFFF",
                TextPrimary = "#FFFFFF",
                TextSecondary = "#FFFFFF",
                TextMuted = "#CCCCCC",
                TextOnAccent = "#000000",
                AccentPrimary = "#FFFFFF",
                AccentSoft = "rgba(255, 255, 255, 0.2)",
                AccentGradient = "linear-gradient(135deg, #FFFFFF, #CCCCCC)",
                ButtonPrimaryBg = "#FFFFFF",
                ButtonPrimaryText = "#000000",
                ButtonPrimaryHover = "#E5E5E5",
                ButtonSecondaryBorder = "#FFFFFF",
                ButtonShape = "pill",
                FocusRing = "rgba(255, 255, 255, 0.8)"
            }
        },
        
        new ThemePreset
        {
            Id = "obsidian",
            Name = "Obsidian",
            Category = "dark",
            Personality = "Minimal, tech",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#0A0A0A",
                BgOverlayOpacity = 0,
                BgIsDark = true,
                SurfaceCard = "rgba(100, 116, 139, 0.08)",
                SurfaceCardBorder = "rgba(100, 116, 139, 0.2)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.4)",
                SurfaceInputBorder = "rgba(100, 116, 139, 0.2)",
                SurfaceInputFocus = "#64748B",
                SurfaceDivider = "rgba(100, 116, 139, 0.15)",
                SurfaceChip = "rgba(100, 116, 139, 0.1)",
                SurfaceChipBorder = "rgba(100, 116, 139, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.8)",
                TextMuted = "rgba(255, 255, 255, 0.5)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#64748B",
                AccentSoft = "rgba(100, 116, 139, 0.2)",
                AccentGradient = "linear-gradient(135deg, #64748B, #475569)",
                ButtonPrimaryBg = "#64748B",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#475569",
                ButtonSecondaryBorder = "rgba(100, 116, 139, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(100, 116, 139, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "aurora",
            Name = "Aurora",
            Category = "dark",
            Personality = "Creative, events",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #0F172A 0%, #1E3A5F 25%, #3B1E5F 50%, #5F1E3A 75%, #0F172A 100%)",
                BgOverlayOpacity = 0.15,
                BgIsDark = true,
                SurfaceCard = "rgba(236, 72, 153, 0.08)",
                SurfaceCardBorder = "rgba(236, 72, 153, 0.2)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.35)",
                SurfaceInputBorder = "rgba(236, 72, 153, 0.15)",
                SurfaceInputFocus = "#EC4899",
                SurfaceDivider = "rgba(236, 72, 153, 0.1)",
                SurfaceChip = "rgba(236, 72, 153, 0.1)",
                SurfaceChipBorder = "rgba(236, 72, 153, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.85)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#EC4899",
                AccentSoft = "rgba(236, 72, 153, 0.2)",
                AccentGradient = "linear-gradient(135deg, #EC4899, #8B5CF6)",
                ButtonPrimaryBg = "#EC4899",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#DB2777",
                ButtonSecondaryBorder = "rgba(236, 72, 153, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(236, 72, 153, 0.5)"
            }
        },
        
        new ThemePreset
        {
            Id = "amber-fire",
            Name = "Amber Fire",
            Category = "dark",
            Personality = "Cotizaciones, ventas",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(180deg, #1a0d05 0%, #2d1a0a 30%, #3d2010 60%, #2d1a0a 100%)",
                BgOverlayOpacity = 0.2,
                BgIsDark = true,
                SurfaceCard = "rgba(245, 158, 11, 0.06)",
                SurfaceCardBorder = "rgba(245, 158, 11, 0.15)",
                SurfaceCardStyle = "glass",
                SurfaceInput = "rgba(0, 0, 0, 0.35)",
                SurfaceInputBorder = "rgba(245, 158, 11, 0.15)",
                SurfaceInputFocus = "#F59E0B",
                SurfaceDivider = "rgba(245, 158, 11, 0.1)",
                SurfaceChip = "rgba(245, 158, 11, 0.1)",
                SurfaceChipBorder = "rgba(245, 158, 11, 0.2)",
                TextPrimary = "#FFFFFF",
                TextSecondary = "rgba(255, 255, 255, 0.85)",
                TextMuted = "rgba(255, 255, 255, 0.6)",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#F59E0B",
                AccentSoft = "rgba(245, 158, 11, 0.2)",
                AccentGradient = "linear-gradient(135deg, #F59E0B, #EF4444)",
                ButtonPrimaryBg = "#F59E0B",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#D97706",
                ButtonSecondaryBorder = "rgba(245, 158, 11, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(245, 158, 11, 0.5)"
            }
        },
        
        // ═══════════════════════════════════════════════════════════════
        // LIGHT PRESETS (8)
        // ═══════════════════════════════════════════════════════════════
        
        new ThemePreset
        {
            Id = "minimal-white",
            Name = "Minimal White",
            Category = "light",
            Personality = "Clean, corporate",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#F8FAFC",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(0, 0, 0, 0.08)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#F1F5F9",
                SurfaceInputBorder = "rgba(0, 0, 0, 0.1)",
                SurfaceInputFocus = "#475569",
                SurfaceDivider = "rgba(0, 0, 0, 0.08)",
                SurfaceChip = "rgba(71, 85, 105, 0.1)",
                SurfaceChipBorder = "rgba(71, 85, 105, 0.2)",
                TextPrimary = "#1E293B",
                TextSecondary = "#475569",
                TextMuted = "#94A3B8",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#475569",
                AccentSoft = "rgba(71, 85, 105, 0.1)",
                AccentGradient = "linear-gradient(135deg, #475569, #334155)",
                ButtonPrimaryBg = "#475569",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#334155",
                ButtonSecondaryBorder = "rgba(71, 85, 105, 0.3)",
                ButtonShape = "pill",
                FocusRing = "rgba(71, 85, 105, 0.3)"
            }
        },
        
        new ThemePreset
        {
            Id = "soft-cream",
            Name = "Soft Cream",
            Category = "light",
            Personality = "Warmth, hospitality",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#FFFBEB",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(217, 119, 6, 0.15)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#FEF3C7",
                SurfaceInputBorder = "rgba(217, 119, 6, 0.2)",
                SurfaceInputFocus = "#D97706",
                SurfaceDivider = "rgba(217, 119, 6, 0.1)",
                SurfaceChip = "rgba(217, 119, 6, 0.1)",
                SurfaceChipBorder = "rgba(217, 119, 6, 0.2)",
                TextPrimary = "#78350F",
                TextSecondary = "#92400E",
                TextMuted = "#B45309",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#D97706",
                AccentSoft = "rgba(217, 119, 6, 0.15)",
                AccentGradient = "linear-gradient(135deg, #D97706, #B45309)",
                ButtonPrimaryBg = "#D97706",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#B45309",
                ButtonSecondaryBorder = "rgba(217, 119, 6, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(217, 119, 6, 0.3)"
            }
        },
        
        new ThemePreset
        {
            Id = "sky-light",
            Name = "Sky Light",
            Category = "light",
            Personality = "Fresh, modern",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #E0F2FE 0%, #BAE6FD 50%, #E0F2FE 100%)",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(2, 132, 199, 0.15)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#F0F9FF",
                SurfaceInputBorder = "rgba(2, 132, 199, 0.2)",
                SurfaceInputFocus = "#0284C7",
                SurfaceDivider = "rgba(2, 132, 199, 0.1)",
                SurfaceChip = "rgba(2, 132, 199, 0.1)",
                SurfaceChipBorder = "rgba(2, 132, 199, 0.2)",
                TextPrimary = "#0C4A6E",
                TextSecondary = "#075985",
                TextMuted = "#0369A1",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#0284C7",
                AccentSoft = "rgba(2, 132, 199, 0.15)",
                AccentGradient = "linear-gradient(135deg, #0284C7, #0369A1)",
                ButtonPrimaryBg = "#0284C7",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#0369A1",
                ButtonSecondaryBorder = "rgba(2, 132, 199, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(2, 132, 199, 0.3)"
            }
        },
        
        new ThemePreset
        {
            Id = "mint-breeze",
            Name = "Mint Breeze",
            Category = "light",
            Personality = "Health, wellness",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#ECFDF5",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(5, 150, 105, 0.15)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#D1FAE5",
                SurfaceInputBorder = "rgba(5, 150, 105, 0.2)",
                SurfaceInputFocus = "#059669",
                SurfaceDivider = "rgba(5, 150, 105, 0.1)",
                SurfaceChip = "rgba(5, 150, 105, 0.1)",
                SurfaceChipBorder = "rgba(5, 150, 105, 0.2)",
                TextPrimary = "#064E3B",
                TextSecondary = "#047857",
                TextMuted = "#059669",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#059669",
                AccentSoft = "rgba(5, 150, 105, 0.15)",
                AccentGradient = "linear-gradient(135deg, #059669, #047857)",
                ButtonPrimaryBg = "#059669",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#047857",
                ButtonSecondaryBorder = "rgba(5, 150, 105, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(5, 150, 105, 0.3)"
            }
        },
        
        new ThemePreset
        {
            Id = "pearl-gray",
            Name = "Pearl Gray",
            Category = "light",
            Personality = "Elegant, professional",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#F1F5F9",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(100, 116, 139, 0.15)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#E2E8F0",
                SurfaceInputBorder = "rgba(100, 116, 139, 0.2)",
                SurfaceInputFocus = "#64748B",
                SurfaceDivider = "rgba(100, 116, 139, 0.1)",
                SurfaceChip = "rgba(100, 116, 139, 0.1)",
                SurfaceChipBorder = "rgba(100, 116, 139, 0.2)",
                TextPrimary = "#1E293B",
                TextSecondary = "#475569",
                TextMuted = "#64748B",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#64748B",
                AccentSoft = "rgba(100, 116, 139, 0.15)",
                AccentGradient = "linear-gradient(135deg, #64748B, #475569)",
                ButtonPrimaryBg = "#64748B",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#475569",
                ButtonSecondaryBorder = "rgba(100, 116, 139, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(100, 116, 139, 0.3)"
            }
        },
        
        new ThemePreset
        {
            Id = "rose-blush",
            Name = "Rose Blush",
            Category = "light",
            Personality = "Gentle, welcoming",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#FFF1F2",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(225, 29, 72, 0.12)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#FFE4E6",
                SurfaceInputBorder = "rgba(225, 29, 72, 0.15)",
                SurfaceInputFocus = "#E11D48",
                SurfaceDivider = "rgba(225, 29, 72, 0.08)",
                SurfaceChip = "rgba(225, 29, 72, 0.08)",
                SurfaceChipBorder = "rgba(225, 29, 72, 0.15)",
                TextPrimary = "#881337",
                TextSecondary = "#9F1239",
                TextMuted = "#BE123C",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#E11D48",
                AccentSoft = "rgba(225, 29, 72, 0.12)",
                AccentGradient = "linear-gradient(135deg, #E11D48, #BE123C)",
                ButtonPrimaryBg = "#E11D48",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#BE123C",
                ButtonSecondaryBorder = "rgba(225, 29, 72, 0.35)",
                ButtonShape = "pill",
                FocusRing = "rgba(225, 29, 72, 0.25)"
            }
        },
        
        new ThemePreset
        {
            Id = "ocean-mist",
            Name = "Ocean Mist",
            Category = "light",
            Personality = "Calm, trustworthy",
            Tokens = new ThemeTokens
            {
                BgType = "gradient",
                BgValue = "linear-gradient(135deg, #F0FDFA 0%, #CCFBF1 50%, #F0FDFA 100%)",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(20, 184, 166, 0.15)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#CCFBF1",
                SurfaceInputBorder = "rgba(20, 184, 166, 0.2)",
                SurfaceInputFocus = "#14B8A6",
                SurfaceDivider = "rgba(20, 184, 166, 0.1)",
                SurfaceChip = "rgba(20, 184, 166, 0.1)",
                SurfaceChipBorder = "rgba(20, 184, 166, 0.2)",
                TextPrimary = "#134E4A",
                TextSecondary = "#115E59",
                TextMuted = "#0D9488",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#14B8A6",
                AccentSoft = "rgba(20, 184, 166, 0.15)",
                AccentGradient = "linear-gradient(135deg, #14B8A6, #0D9488)",
                ButtonPrimaryBg = "#14B8A6",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#0D9488",
                ButtonSecondaryBorder = "rgba(20, 184, 166, 0.4)",
                ButtonShape = "pill",
                FocusRing = "rgba(20, 184, 166, 0.3)"
            }
        },
        
        new ThemePreset
        {
            Id = "lavender-cloud",
            Name = "Lavender Cloud",
            Category = "light",
            Personality = "Creative, serene",
            Tokens = new ThemeTokens
            {
                BgType = "solid",
                BgValue = "#FAF5FF",
                BgOverlayOpacity = 0,
                BgIsDark = false,
                SurfaceCard = "#FFFFFF",
                SurfaceCardBorder = "rgba(147, 51, 234, 0.12)",
                SurfaceCardStyle = "elevated",
                SurfaceInput = "#F3E8FF",
                SurfaceInputBorder = "rgba(147, 51, 234, 0.15)",
                SurfaceInputFocus = "#9333EA",
                SurfaceDivider = "rgba(147, 51, 234, 0.08)",
                SurfaceChip = "rgba(147, 51, 234, 0.08)",
                SurfaceChipBorder = "rgba(147, 51, 234, 0.15)",
                TextPrimary = "#581C87",
                TextSecondary = "#6B21A8",
                TextMuted = "#7E22CE",
                TextOnAccent = "#FFFFFF",
                AccentPrimary = "#9333EA",
                AccentSoft = "rgba(147, 51, 234, 0.12)",
                AccentGradient = "linear-gradient(135deg, #9333EA, #7E22CE)",
                ButtonPrimaryBg = "#9333EA",
                ButtonPrimaryText = "#FFFFFF",
                ButtonPrimaryHover = "#7E22CE",
                ButtonSecondaryBorder = "rgba(147, 51, 234, 0.35)",
                ButtonShape = "pill",
                FocusRing = "rgba(147, 51, 234, 0.25)"
            }
        }
    };
    
    /// <summary>
    /// Get preset by ID, returns null if not found
    /// </summary>
    public static ThemePreset? GetById(string id) => All.FirstOrDefault(p => p.Id == id);
    
    /// <summary>
    /// Get default preset (Premium Dark)
    /// </summary>
    public static ThemePreset Default => All.First(p => p.Id == "premium-dark");
    
    /// <summary>
    /// Get all dark presets
    /// </summary>
    public static IEnumerable<ThemePreset> DarkPresets => All.Where(p => p.Category == "dark");
    
    /// <summary>
    /// Get all light presets
    /// </summary>
    public static IEnumerable<ThemePreset> LightPresets => All.Where(p => p.Category == "light");
}
