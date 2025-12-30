using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DataTouch.Web.Services;

/// <summary>
/// Seeder para plantillas de tarjetas digitales.
/// Los templates se crearán uno a uno según indicaciones del usuario.
/// </summary>
public static class CardTemplateSeeder
{
    public static async Task SeedTemplatesAsync(DataTouchDbContext context)
    {
        // Skip if templates already exist
        if (await context.CardTemplates.AnyAsync())
        {
            return;
        }

        var templates = GetSystemTemplates();
        if (templates.Any())
        {
            context.CardTemplates.AddRange(templates);
            await context.SaveChangesAsync();
        }
    }

    private static List<CardTemplate> GetSystemTemplates()
    {
        var now = DateTime.UtcNow;
        
        return new List<CardTemplate>
        {
            // ═══════════════════════════════════════════════════════════════
            // TEMPLATE 1: CORAL WAVE - Premium Business Card
            // ═══════════════════════════════════════════════════════════════
            new CardTemplate
            {
                Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Name = "Coral Wave",
                Industry = "Business",
                Description = "Template premium con hero, wave separator, monograma y dock flotante. Ideal para profesionales y ejecutivos.",
                ThumbnailUrl = "/images/templates/coral-wave-thumb.png",
                IsSystemTemplate = true,
                IsActive = true,
                CreatedAt = now,
                DefaultStyleJson = JsonSerializer.Serialize(new
                {
                    LayoutType = "coral-wave",
                    PrimaryColor = "#021B47",
                    AccentColor = "#E64F58",
                    BackgroundColor = "#FBE3E5",
                    SurfaceColor = "#FFFFFF",
                    TextPrimaryColor = "#0B0F1A",
                    TextSecondaryColor = "#6B7280",
                    FontFamily = "Inter, sans-serif",
                    CardRadius = "20px",
                    CardShadow = "0 4px 20px rgba(0, 0, 0, 0.08)",
                    HeroHeight = "45vh",
                    MonogramSize = "80px"
                }),
                DefaultComponentsJson = JsonSerializer.Serialize(new[]
                {
                    new { Type = "hero", Order = 0, Enabled = true },
                    new { Type = "wave-separator", Order = 1, Enabled = true },
                    new { Type = "about-me", Order = 2, Enabled = true },
                    new { Type = "contact-us", Order = 3, Enabled = true },
                    new { Type = "web-links", Order = 4, Enabled = true },
                    new { Type = "schedule-meeting", Order = 5, Enabled = true },
                    new { Type = "floating-dock", Order = 99, Enabled = true }
                })
            },
            
            // ═══════════════════════════════════════════════════════════════
            // TEMPLATE 2: PORTFOLIO CREATIVE - Gallery-focused Card
            // ═══════════════════════════════════════════════════════════════
            new CardTemplate
            {
                Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f23456789012"),
                Name = "Portafolio Creativo",
                Industry = "Creative",
                Description = "Template con galería de imágenes para creativos, diseñadores y artistas. Muestra tu trabajo visualmente.",
                ThumbnailUrl = "/images/templates/portfolio-creative-thumb.png",
                IsSystemTemplate = true,
                IsActive = true,
                CreatedAt = now,
                DefaultStyleJson = JsonSerializer.Serialize(new
                {
                    LayoutType = "portfolio-creative",
                    PrimaryColor = "#8B5CF6",
                    AccentColor = "#EC4899",
                    BackgroundColor = "#0F172A",
                    SurfaceColor = "#1E293B",
                    TextPrimaryColor = "#F8FAFC",
                    TextSecondaryColor = "#94A3B8",
                    FontFamily = "Inter, sans-serif",
                    CardRadius = "16px",
                    CardShadow = "0 8px 32px rgba(0, 0, 0, 0.3)",
                    GalleryColumns = 2,
                    GalleryGap = "12px"
                }),
                DefaultComponentsJson = JsonSerializer.Serialize(new[]
                {
                    new { Type = "hero", Order = 0, Enabled = true },
                    new { Type = "about-me", Order = 1, Enabled = true },
                    new { Type = "gallery", Order = 2, Enabled = true },
                    new { Type = "contact-cta", Order = 3, Enabled = true },
                    new { Type = "social-links", Order = 4, Enabled = true }
                })
            }
        };
    }
}

