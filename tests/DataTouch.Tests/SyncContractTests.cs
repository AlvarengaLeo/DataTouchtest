using DataTouch.Web.Models;
using DataTouch.Web.Services;

namespace DataTouch.Tests;

/// <summary>
/// Sync contract tests — verify shared models and services exist and behave consistently.
/// Guards against re-introduction of duplicated private classes.
/// </summary>
public class SyncContractTests
{
    [Fact]
    public void CardStyleModel_IsShared_NotPrivate()
    {
        // CardStyleModel must be public in DataTouch.Web.Models namespace
        var type = typeof(CardStyleModel);
        Assert.True(type.IsPublic);
        Assert.Equal("DataTouch.Web.Models", type.Namespace);
    }

    [Fact]
    public void CardStyleModel_HasAllRequiredProperties()
    {
        var props = typeof(CardStyleModel).GetProperties();
        var names = props.Select(p => p.Name).ToHashSet();

        // Core properties that both MyCard and PublicCard need
        Assert.Contains("BackgroundType", names);
        Assert.Contains("BackgroundValue", names);
        Assert.Contains("CardContainerStyle", names);
        Assert.Contains("GlassIntensity", names);
        Assert.Contains("ButtonShape", names);
        Assert.Contains("ButtonStyle", names);
        Assert.Contains("BackgroundIsDark", names);
        Assert.Contains("CardIsDark", names);
        Assert.Contains("PresetId", names);
        Assert.Contains("AccentColor", names);
    }

    [Fact]
    public void CardService_GetDefaultPreset_ReturnsCorrectDefaults()
    {
        Assert.Equal("sky-light", CardService.GetDefaultPresetForTemplate("quote-request"));
        Assert.Equal("emerald-night", CardService.GetDefaultPresetForTemplate("services-quotes"));
        Assert.Equal("premium-dark", CardService.GetDefaultPresetForTemplate("default"));
    }

    [Fact]
    public void CardService_DeserializeStyle_HandlesNullGracefully()
    {
        var style = CardService.DeserializeStyle(null);
        Assert.NotNull(style);
        Assert.Equal("pill", style.ButtonShape);
    }

    [Fact]
    public void CardService_RoundTrip_PreservesAllProperties()
    {
        var original = new CardStyleModel
        {
            BackgroundType = "solid",
            ButtonShape = "square",
            GlassIntensity = 75,
            PresetId = "sky-light",
            AccentColor = "#0284C7"
        };

        var json = CardService.SerializeStyle(original);
        var deserialized = CardService.DeserializeStyle(json);

        Assert.Equal(original.BackgroundType, deserialized.BackgroundType);
        Assert.Equal(original.ButtonShape, deserialized.ButtonShape);
        Assert.Equal(original.GlassIntensity, deserialized.GlassIntensity);
        Assert.Equal(original.PresetId, deserialized.PresetId);
        Assert.Equal(original.AccentColor, deserialized.AccentColor);
    }

    [Fact]
    public void ThemeHelper_GenerateCssVariables_ContainsBridgeAliases()
    {
        var tokens = PresetRegistry.Default.Tokens;
        var css = ThemeHelper.GenerateCssVariables(tokens);

        // Must contain --dt-* canonical vars
        Assert.Contains("--dt-accent-primary:", css);
        Assert.Contains("--dt-text-primary:", css);
        Assert.Contains("--dt-button-primary-bg:", css);

        // Must contain --surface-* bridge aliases
        Assert.Contains("--surface-text-primary:", css);
        Assert.Contains("--surface-chip-bg:", css);
        Assert.Contains("--surface-input-bg:", css);
    }

    [Fact]
    public void PresetRegistry_SkyLight_Exists()
    {
        var preset = PresetRegistry.GetById("sky-light");
        Assert.NotNull(preset);
        Assert.False(preset.Tokens.BgIsDark);
    }

    [Fact]
    public void PresetRegistry_EmeraldNight_Exists()
    {
        var preset = PresetRegistry.GetById("emerald-night");
        Assert.NotNull(preset);
        Assert.True(preset.Tokens.BgIsDark);
    }
}
