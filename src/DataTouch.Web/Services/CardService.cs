using System.Text.Json;
using DataTouch.Web.Models;

namespace DataTouch.Web.Services;

/// <summary>
/// Shared service for card style serialization and theme defaults.
/// Single source of truth for loading/saving CardStyleModel and applying preset defaults.
/// </summary>
public static class CardService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializes AppearanceStyleJson into CardStyleModel, returning defaults if null/invalid.
    /// </summary>
    public static CardStyleModel DeserializeStyle(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new CardStyleModel();

        try
        {
            return JsonSerializer.Deserialize<CardStyleModel>(json, _jsonOptions) ?? new CardStyleModel();
        }
        catch
        {
            return new CardStyleModel();
        }
    }

    /// <summary>
    /// Serializes CardStyleModel to JSON for persistence.
    /// </summary>
    public static string SerializeStyle(CardStyleModel style)
    {
        return JsonSerializer.Serialize(style, _jsonOptions);
    }

    /// <summary>
    /// Returns the default preset ID for a given template type.
    /// </summary>
    public static string GetDefaultPresetForTemplate(string templateType) => templateType switch
    {
        "quote-request" => "sky-light",
        "services-quotes" => "emerald-night",
        "appointments" => "mint-breeze",
        "reservations-range" => "soft-cream",
        _ => "premium-dark"
    };

    /// <summary>
    /// Loads ThemeTokens for a given preset ID, falling back to PresetRegistry.Default.
    /// </summary>
    public static ThemeTokens GetThemeTokens(string? presetId)
    {
        var preset = PresetRegistry.GetById(presetId ?? "premium-dark") ?? PresetRegistry.Default;
        return preset.Tokens;
    }
}
