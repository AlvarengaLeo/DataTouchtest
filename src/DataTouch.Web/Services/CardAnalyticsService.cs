using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Servicio para registrar y consultar analíticas de tarjetas digitales.
/// Registra eventos como escaneos de QR, clics en enlaces, envío de formularios, etc.
/// </summary>
public class CardAnalyticsService
{
    private readonly DataTouchDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CardAnalyticsService(DataTouchDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Registra un evento de visualización de página
    /// </summary>
    public async Task TrackPageViewAsync(Guid cardId)
    {
        await TrackEventAsync(cardId, "page_view");
    }

    /// <summary>
    /// Registra un escaneo de código QR
    /// </summary>
    public async Task TrackQrScanAsync(Guid cardId)
    {
        await TrackEventAsync(cardId, "qr_scan");
    }

    /// <summary>
    /// Registra un clic en un enlace o red social
    /// </summary>
    public async Task TrackLinkClickAsync(Guid cardId, string linkType, string? url = null)
    {
        var metadata = new { link_type = linkType, url = url };
        await TrackEventAsync(cardId, "link_click", System.Text.Json.JsonSerializer.Serialize(metadata));
    }

    /// <summary>
    /// Registra un clic en un botón CTA (WhatsApp, Llamar, Email)
    /// </summary>
    public async Task TrackCtaClickAsync(Guid cardId, string buttonType)
    {
        var metadata = new { button = buttonType };
        await TrackEventAsync(cardId, "cta_click", System.Text.Json.JsonSerializer.Serialize(metadata));
    }

    /// <summary>
    /// Registra cuando el visitante guarda el contacto
    /// </summary>
    public async Task TrackContactSaveAsync(Guid cardId)
    {
        await TrackEventAsync(cardId, "contact_save");
    }

    /// <summary>
    /// Registra el envío de un formulario de contacto
    /// </summary>
    public async Task TrackFormSubmitAsync(Guid cardId, Guid? leadId = null)
    {
        var metadata = leadId.HasValue ? new { lead_id = leadId.Value.ToString() } : null;
        await TrackEventAsync(cardId, "form_submit", metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null);
    }

    /// <summary>
    /// Registra cuando la tarjeta es compartida
    /// </summary>
    public async Task TrackShareAsync(Guid cardId, string? method = null)
    {
        var metadata = !string.IsNullOrEmpty(method) ? new { method = method } : null;
        await TrackEventAsync(cardId, "share", metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null);
    }

    // ═══════════════════════════════════════════════════════════════
    // QUOTE REQUEST TEMPLATE EVENTS
    // ═══════════════════════════════════════════════════════════════

    public async Task TrackQuoteCtaClickAsync(Guid cardId) =>
        await TrackEventAsync(cardId, "quote_cta_click");

    public async Task TrackQuoteFormStartAsync(Guid cardId) =>
        await TrackEventAsync(cardId, "quote_form_start");

    public async Task TrackQuoteModalOpenAsync(Guid cardId) =>
        await TrackEventAsync(cardId, "quote_modal_open");

    public async Task TrackQuoteSubmitAttemptAsync(Guid cardId) =>
        await TrackEventAsync(cardId, "quote_submit_attempt");

    public async Task TrackQuoteSubmitSuccessAsync(Guid cardId, Guid? quoteId = null) =>
        await TrackEventAsync(cardId, "quote_submit_success",
            quoteId.HasValue ? System.Text.Json.JsonSerializer.Serialize(new { quote_id = quoteId }) : null);

    public async Task TrackQuoteSubmitErrorAsync(Guid cardId, string? error = null) =>
        await TrackEventAsync(cardId, "quote_submit_error",
            !string.IsNullOrEmpty(error) ? System.Text.Json.JsonSerializer.Serialize(new { error }) : null);

    // ═══════════════════════════════════════════════════════════════
    // APPOINTMENTS TEMPLATE EVENTS
    // ═══════════════════════════════════════════════════════════════

    public async Task TrackSchedulerOpenAsync(Guid cardId) =>
        await TrackEventAsync(cardId, "scheduler_open");

    public async Task TrackSchedulerStepAsync(Guid cardId, int step) =>
        await TrackEventAsync(cardId, "scheduler_step",
            System.Text.Json.JsonSerializer.Serialize(new { step }));

    public async Task TrackAppointmentBookedAsync(Guid cardId, Guid? appointmentId = null) =>
        await TrackEventAsync(cardId, "appointment_booked",
            appointmentId.HasValue ? System.Text.Json.JsonSerializer.Serialize(new { appointment_id = appointmentId }) : null);

    public async Task TrackAppointmentErrorAsync(Guid cardId, string? error = null) =>
        await TrackEventAsync(cardId, "appointment_error",
            !string.IsNullOrEmpty(error) ? System.Text.Json.JsonSerializer.Serialize(new { error }) : null);

    /// <summary>
    /// Método genérico para registrar cualquier evento
    /// </summary>
    private async Task TrackEventAsync(Guid cardId, string eventType, string? metadataJson = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        
        var analytics = new CardAnalytics
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            IpAddress = GetClientIpAddress(httpContext),
            Referrer = httpContext?.Request.Headers.Referer.ToString(),
            DeviceType = DetectDeviceType(httpContext?.Request.Headers.UserAgent.ToString()),
            MetadataJson = metadataJson
        };

        _context.CardAnalytics.Add(analytics);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene estadísticas de una tarjeta
    /// </summary>
    public async Task<CardStats> GetCardStatsAsync(Guid cardId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.CardAnalytics
            .Where(a => a.CardId == cardId);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var events = await query.ToListAsync();

        return new CardStats
        {
            TotalViews = events.Count(e => e.EventType == "page_view"),
            QrScans = events.Count(e => e.EventType == "qr_scan"),
            LinkClicks = events.Count(e => e.EventType == "link_click"),
            CtaClicks = events.Count(e => e.EventType == "cta_click"),
            ContactSaves = events.Count(e => e.EventType == "contact_save"),
            FormSubmits = events.Count(e => e.EventType == "form_submit"),
            Shares = events.Count(e => e.EventType == "share"),
            UniqueVisitors = events.Select(e => e.IpAddress).Where(ip => !string.IsNullOrEmpty(ip)).Distinct().Count(),
            DeviceBreakdown = events
                .Where(e => !string.IsNullOrEmpty(e.DeviceType))
                .GroupBy(e => e.DeviceType!)
                .ToDictionary(g => g.Key, g => g.Count()),
            DailyViews = events
                .Where(e => e.EventType == "page_view")
                .GroupBy(e => e.Timestamp.Date)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// Obtiene los eventos recientes de una tarjeta
    /// </summary>
    public async Task<List<CardAnalytics>> GetRecentEventsAsync(Guid cardId, int count = 50)
    {
        return await _context.CardAnalytics
            .Where(a => a.CardId == cardId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    // QUOTE ANALYTICS QUERIES (for /analytics/quotes dashboard)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets quote funnel counts for a date range, optionally filtered by cardId.
    /// Returns counts for: page_view, quote_cta_click, quote_modal_open, quote_form_start, quote_submit_attempt, quote_submit_success, quote_submit_error
    /// </summary>
    public async Task<Dictionary<string, int>> GetQuoteFunnelAsync(DateTime from, DateTime to, Guid? cardId = null)
    {
        var eventTypes = new[] { "page_view", "quote_cta_click", "quote_modal_open", "quote_form_start", "quote_submit_attempt", "quote_submit_success", "quote_submit_error" };

        var query = _context.CardAnalytics
            .Where(a => a.Timestamp >= from && a.Timestamp < to)
            .Where(a => eventTypes.Contains(a.EventType));

        if (cardId.HasValue)
            query = query.Where(a => a.CardId == cardId.Value);

        var grouped = await query
            .GroupBy(a => a.EventType)
            .Select(g => new { EventType = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = eventTypes.ToDictionary(e => e, e => 0);
        foreach (var g in grouped)
            result[g.EventType] = g.Count;

        return result;
    }

    /// <summary>
    /// Gets daily event counts for time series charts.
    /// </summary>
    public async Task<List<DailyEventCount>> GetDailyQuoteEventsAsync(DateTime from, DateTime to, Guid? cardId = null)
    {
        var relevantEvents = new[] { "page_view", "quote_cta_click", "quote_modal_open", "quote_submit_success", "quote_submit_error" };

        var query = _context.CardAnalytics
            .Where(a => a.Timestamp >= from && a.Timestamp < to)
            .Where(a => relevantEvents.Contains(a.EventType));

        if (cardId.HasValue)
            query = query.Where(a => a.CardId == cardId.Value);

        return await query
            .GroupBy(a => new { a.Timestamp.Date, a.EventType })
            .Select(g => new DailyEventCount { Date = g.Key.Date, EventType = g.Key.EventType, Count = g.Count() })
            .OrderBy(d => d.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets quote metadata aggregations (preferred contact, deadline hints).
    /// Reads from QuoteRequests table directly.
    /// </summary>
    public async Task<QuoteMetadataAggregation> GetQuoteMetadataAsync(DateTime from, DateTime to, Guid? cardId = null)
    {
        var query = _context.Set<DataTouch.Domain.Entities.QuoteRequest>()
            .Where(q => q.CreatedAt >= from && q.CreatedAt < to);

        if (cardId.HasValue)
            query = query.Where(q => q.CardId == cardId.Value);

        var quotes = await query.Select(q => new
        {
            q.CustomFieldsJson,
            q.Status
        }).ToListAsync();

        var preferredContacts = new List<string>();
        var deadlines = new List<string>();

        foreach (var q in quotes)
        {
            if (!string.IsNullOrEmpty(q.CustomFieldsJson))
            {
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(q.CustomFieldsJson);
                    if (doc.RootElement.TryGetProperty("preferredContact", out var pc) && pc.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var val = pc.GetString();
                        if (!string.IsNullOrEmpty(val)) preferredContacts.Add(val);
                    }
                    if (doc.RootElement.TryGetProperty("deadline", out var dl) && dl.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        var val = dl.GetString();
                        if (!string.IsNullOrEmpty(val)) deadlines.Add(val);
                    }
                }
                catch { /* ignore malformed JSON */ }
            }
        }

        return new QuoteMetadataAggregation
        {
            PreferredContactBreakdown = preferredContacts
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count()),
            DeadlineBreakdown = deadlines
                .GroupBy(d => d)
                .ToDictionary(g => g.Key, g => g.Count()),
            StatusBreakdown = quotes
                .GroupBy(q => q.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    /// <summary>
    /// Gets all cards that have quote-related events for the card filter dropdown.
    /// </summary>
    public async Task<List<CardFilterItem>> GetCardsWithQuoteEventsAsync()
    {
        var quoteEventTypes = new[] { "quote_cta_click", "quote_modal_open", "quote_form_start", "quote_submit_attempt", "quote_submit_success" };

        var cardIds = await _context.CardAnalytics
            .Where(a => quoteEventTypes.Contains(a.EventType))
            .Select(a => a.CardId)
            .Distinct()
            .ToListAsync();

        var cards = await _context.Cards
            .Where(c => cardIds.Contains(c.Id))
            .Select(c => new CardFilterItem { Id = c.Id, FullName = c.FullName ?? "Sin nombre", Slug = c.Slug })
            .ToListAsync();

        return cards;
    }

    private string? GetClientIpAddress(HttpContext? context)
    {
        if (context == null) return null;

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    private string DetectDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "unknown";

        userAgent = userAgent.ToLower();

        if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            return "mobile";
        
        if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
            return "tablet";

        return "desktop";
    }
}

/// <summary>
/// Estadísticas agregadas de una tarjeta
/// </summary>
public class CardStats
{
    public int TotalViews { get; set; }
    public int QrScans { get; set; }
    public int LinkClicks { get; set; }
    public int CtaClicks { get; set; }
    public int ContactSaves { get; set; }
    public int FormSubmits { get; set; }
    public int Shares { get; set; }
    public int UniqueVisitors { get; set; }
    public Dictionary<string, int> DeviceBreakdown { get; set; } = new();
    public Dictionary<DateTime, int> DailyViews { get; set; } = new();
}

public class DailyEventCount
{
    public DateTime Date { get; set; }
    public string EventType { get; set; } = "";
    public int Count { get; set; }
}

public class QuoteMetadataAggregation
{
    public Dictionary<string, int> PreferredContactBreakdown { get; set; } = new();
    public Dictionary<string, int> DeadlineBreakdown { get; set; } = new();
    public Dictionary<string, int> StatusBreakdown { get; set; } = new();
}

public class CardFilterItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string? Slug { get; set; }
}
