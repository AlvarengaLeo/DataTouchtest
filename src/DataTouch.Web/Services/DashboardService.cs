using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DataTouch.Web.Services;

/// <summary>
/// Dashboard analytics service providing KPIs, trends, and insights.
/// All metrics support date range filtering and period comparison.
/// 
/// CONSISTENCY RULES:
/// - All widgets use the same date range from GetDateRangeFromOption
/// - Interactions = CardAnalytics events of types: page_view, qr_scan, cta_click, contact_save, form_submit
/// - Leads = Lead entities created in date range
/// - Conversion Rate = Leads / Interactions (N/A if Interactions = 0)
/// - High-Intent = contact_save + form_submit + meeting/calendar CTAs + call CTAs + whatsapp CTAs
/// </summary>
public class DashboardService
{
    private readonly IDbContextFactory<DataTouchDbContext> _contextFactory;
    
    // Event types considered as interactions
    private static readonly string[] InteractionEventTypes = 
        { "page_view", "qr_scan", "cta_click", "contact_save", "form_submit" };
    
    // High-intent event types and CTA buttons
    private static readonly string[] HighIntentEventTypes = { "contact_save", "form_submit" };
    private static readonly string[] HighIntentCtaButtons = { "whatsapp", "call", "calendar", "booking", "meeting" };

    public DashboardService(IDbContextFactory<DataTouchDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    #region Data Models

    public record DateRangeFilter(DateTime Start, DateTime End)
    {
        public int DayCount => (int)(End - Start).TotalDays + 1;
        
        public DateRangeFilter GetPreviousPeriod()
        {
            var days = DayCount;
            return new DateRangeFilter(Start.AddDays(-days), Start.AddDays(-1));
        }
    }

    public record KpiValue(decimal Value, decimal PreviousValue, decimal DeltaPercent, bool IsPositive, bool HasData = true);
    
    public record DashboardKpis(
        KpiValue TotalInteractions,
        KpiValue LeadsCaptured,
        KpiValue MeetingsBooked,
        KpiValue ConversionRate
    );

    public record ChartDataPoint(DateTime Date, int Interactions, int Leads);
    
    public record LocationData(
        string Location,
        string Country,
        string CountryCode,
        int Interactions,
        int Leads,
        decimal ConversionRate,
        bool HasConversion,
        double? Latitude,
        double? Longitude
    );

    public record MapPoint(
        string Location,
        double Latitude,
        double Longitude,
        int Weight,
        int Leads,
        decimal ConversionRate,
        decimal Delta,
        bool IsTopLocation
    );

    public record TopLocationsResult(
        string TopLocation,
        string TopLocationCountryCode,
        decimal TopLocationDelta,
        bool HasData,
        List<LocationData> Locations,
        List<MapPoint> MapPoints,
        double? CenterLat,
        double? CenterLng,
        int DefaultZoom,
        int PendingGeoCount
    );

    public record LinkPerformance(
        string LinkName,
        string Icon,
        int Clicks,
        decimal ConversionRate,
        decimal Trend,
        bool HasConversion
    );

    public record InsightsData(
        string TopCta,
        decimal TopCtaConversion,
        decimal TopCtaDelta,
        bool HasTopCta,
        int MedianTimeToLeadMinutes,
        decimal MedianTimeDelta,
        bool HasMedianTime,
        int InactiveCardsCount,
        decimal InactiveCardsPercent,
        int PotentialLossInteractions,
        int TotalActiveCards
    );

    public record HighIntentData(
        int TotalHighIntent,
        int ContactsSaved,
        int MeetingsBooked,
        int WhatsAppClicks,
        int CallClicks,
        int FormSubmits,
        List<RecentActivity> RecentActivities
    );

    public record RecentActivity(
        string Description,
        string TimeAgo,
        Guid? LeadId,
        string EventType,
        string? ContactName
    );

    public enum ChartAggregation { Day, Week, Month }
    public enum LocationSortBy { Conversion, Leads, Interactions }

    #endregion

    #region Main Dashboard Data

    public async Task<DashboardKpis> GetDashboardKpisAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        var prevPeriod = dateRange.GetPreviousPeriod();

        // Current period data
        var currentInteractions = await GetTotalInteractionsAsync(organizationId, dateRange);
        var currentLeads = await GetLeadsCapturedAsync(organizationId, dateRange);
        var currentMeetings = await GetMeetingsBookedAsync(organizationId, dateRange);

        // Previous period data
        var prevInteractions = await GetTotalInteractionsAsync(organizationId, prevPeriod);
        var prevLeads = await GetLeadsCapturedAsync(organizationId, prevPeriod);
        var prevMeetings = await GetMeetingsBookedAsync(organizationId, prevPeriod);

        // Calculate conversion rates (N/A if no interactions)
        var hasCurrentData = currentInteractions > 0;
        var hasPrevData = prevInteractions > 0;
        
        var currentConversion = hasCurrentData ? (decimal)currentLeads / currentInteractions * 100 : 0;
        var prevConversion = hasPrevData ? (decimal)prevLeads / prevInteractions * 100 : 0;

        return new DashboardKpis(
            CreateKpiValue(currentInteractions, prevInteractions, higherIsBetter: true),
            CreateKpiValue(currentLeads, prevLeads, higherIsBetter: true),
            CreateKpiValue(currentMeetings, prevMeetings, higherIsBetter: true),
            CreateKpiValue(currentConversion, prevConversion, higherIsBetter: true, hasData: hasCurrentData)
        );
    }

    private KpiValue CreateKpiValue(decimal current, decimal previous, bool higherIsBetter, bool hasData = true)
    {
        var delta = previous > 0 ? (current - previous) / previous * 100 : (current > 0 ? 100 : 0);
        var isPositive = higherIsBetter ? delta >= 0 : delta <= 0;
        return new KpiValue(current, previous, Math.Round(delta, 1), isPositive, hasData);
    }

    #endregion

    #region Interactions & Leads

    private async Task<int> GetTotalInteractionsAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        if (!cardIds.Any()) return 0;
        
        var endDate = dateRange.End.AddDays(1);
        var eventTypes = InteractionEventTypes.ToList();

        return await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
            .Where(a => eventTypes.Contains(a.EventType))
            .CountAsync();
    }

    private async Task<int> GetLeadsCapturedAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var endDate = dateRange.End.AddDays(1);
        return await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= dateRange.Start && l.CreatedAt < endDate)
            .CountAsync();
    }

    private async Task<int> GetMeetingsBookedAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        if (!cardIds.Any()) return 0;
        
        var endDate = dateRange.End.AddDays(1);
        
        // Count CTA clicks for calendar/booking/meeting actions
        var meetingEvents = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
            .Where(a => a.EventType == "cta_click" && a.MetadataJson != null)
            .Select(a => a.MetadataJson)
            .ToListAsync();

        return meetingEvents.Count(m => 
            m != null && 
            (m.Contains("calendar", StringComparison.OrdinalIgnoreCase) || 
             m.Contains("booking", StringComparison.OrdinalIgnoreCase) || 
             m.Contains("meeting", StringComparison.OrdinalIgnoreCase)));
    }

    #endregion

    #region Chart Data

    public async Task<List<ChartDataPoint>> GetInteractionsVsLeadsChartAsync(
        Guid organizationId, 
        DateRangeFilter dateRange, 
        ChartAggregation aggregation)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);

        // Get all interactions in range (using same definition as KPIs)
        var endDate = dateRange.End.AddDays(1);
        var eventTypes = InteractionEventTypes.ToList();

        var interactions = cardIds.Any() 
            ? await context.CardAnalytics
                .Where(a => cardIds.Contains(a.CardId))
                .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
                .Where(a => eventTypes.Contains(a.EventType))
                .Select(a => a.Timestamp)
                .ToListAsync()
            : new List<DateTime>();

        // Get all leads in range (using same definition as KPIs)
        var leads = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= dateRange.Start && l.CreatedAt < endDate)
            .Select(l => l.CreatedAt)
            .ToListAsync();

        // Aggregate based on selected period
        var result = new List<ChartDataPoint>();
        var current = dateRange.Start;
        var processedDates = new HashSet<DateTime>();
        
        while (current <= dateRange.End)
        {
            DateTime periodStart;
            DateTime periodEnd;
            DateTime nextPeriod;

            switch (aggregation)
            {
                case ChartAggregation.Week:
                    var diff = (7 + (current.DayOfWeek - DayOfWeek.Monday)) % 7;
                    periodStart = current.AddDays(-diff).Date;
                    periodEnd = periodStart.AddDays(6);
                    nextPeriod = periodStart.AddDays(7);
                    break;
                    
                case ChartAggregation.Month:
                    periodStart = new DateTime(current.Year, current.Month, 1);
                    periodEnd = periodStart.AddMonths(1).AddDays(-1);
                    nextPeriod = periodStart.AddMonths(1);
                    break;
                    
                default: // Day
                    periodStart = current.Date;
                    periodEnd = current.Date;
                    nextPeriod = current.AddDays(1);
                    break;
            }

            // Avoid duplicates
            if (!processedDates.Contains(periodStart))
            {
                processedDates.Add(periodStart);
                
                if (periodEnd > dateRange.End) periodEnd = dateRange.End;

                var interactionCount = interactions.Count(i => i.Date >= periodStart && i.Date <= periodEnd);
                var leadCount = leads.Count(l => l.Date >= periodStart && l.Date <= periodEnd);

                result.Add(new ChartDataPoint(periodStart, interactionCount, leadCount));
            }
            
            current = nextPeriod;
            if (current > dateRange.End) break;
        }

        return result.OrderBy(r => r.Date).ToList();
    }

    #endregion

    #region Top Locations

    public async Task<TopLocationsResult> GetTopLocationsAsync(
        Guid organizationId, 
        DateRangeFilter dateRange,
        LocationSortBy sortBy = LocationSortBy.Conversion)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        var prevPeriod = dateRange.GetPreviousPeriod();

        if (!cardIds.Any())
        {
            return new TopLocationsResult("N/A", "", 0, false, new List<LocationData>(), new List<MapPoint>(), null, null, 2, 0);
        }

        // Get current period events with location data
        var endDate = dateRange.End.AddDays(1);
        var eventTypes = InteractionEventTypes.ToList();

        var currentEvents = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
            .Where(a => eventTypes.Contains(a.EventType))
            .ToListAsync();

        if (!currentEvents.Any())
        {
            return new TopLocationsResult("No data", "", 0, false, new List<LocationData>(), new List<MapPoint>(), null, null, 2, 0);
        }

        // Get leads for conversion calculation
        var leadsInRange = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= dateRange.Start && l.CreatedAt < endDate)
            .ToListAsync();

        // Group by location (city + country or just country)
        var locationGroups = currentEvents
            .GroupBy(e => GetLocationKey(e.City, e.Country))
            .Where(g => g.Key != "Unknown")
            .Select(g =>
            {
                var interactions = g.Count();
                var cardIdsInLocation = g.Select(e => e.CardId).Distinct().ToList();
                var leads = leadsInRange.Count(l => cardIdsInLocation.Contains(l.CardId));
                var hasConversion = interactions > 0;
                var conversion = hasConversion ? Math.Round((decimal)leads / interactions * 100, 1) : 0;
                
                // Get coordinates from first event with coords, or lookup
                var eventWithCoords = g.FirstOrDefault(e => e.Latitude.HasValue && e.Longitude.HasValue);
                double? lat = eventWithCoords?.Latitude;
                double? lng = eventWithCoords?.Longitude;
                
                // Fallback to known location lookup if no coords
                if (!lat.HasValue || !lng.HasValue)
                {
                    var (fallbackLat, fallbackLng) = GetKnownLocationCoords(g.First().City, g.First().CountryCode ?? GetCountryCode(g.First().Country));
                    lat = fallbackLat;
                    lng = fallbackLng;
                }
                
                return new LocationData(
                    g.Key,
                    g.First().Country ?? "Unknown",
                    g.First().CountryCode ?? GetCountryCode(g.First().Country),
                    interactions,
                    leads,
                    conversion,
                    hasConversion,
                    lat,
                    lng
                );
            })
            .ToList();

        // Add "Unknown" if there are unlocated events
        var unknownEvents = currentEvents.Where(e => string.IsNullOrEmpty(e.City) && string.IsNullOrEmpty(e.Country)).ToList();
        if (unknownEvents.Any())
        {
            var unknownInteractions = unknownEvents.Count;
            var unknownCardIds = unknownEvents.Select(e => e.CardId).Distinct().ToList();
            var unknownLeads = leadsInRange.Count(l => unknownCardIds.Contains(l.CardId));
            locationGroups.Add(new LocationData(
                "Unknown",
                "Unknown",
                "",
                unknownInteractions,
                unknownLeads,
                unknownInteractions > 0 ? Math.Round((decimal)unknownLeads / unknownInteractions * 100, 1) : 0,
                unknownInteractions > 0,
                null,
                null
            ));
        }

        // Sort based on selected criteria
        locationGroups = sortBy switch
        {
            LocationSortBy.Leads => locationGroups.OrderByDescending(l => l.Leads).ThenByDescending(l => l.Interactions).ToList(),
            LocationSortBy.Interactions => locationGroups.OrderByDescending(l => l.Interactions).ToList(),
            _ => locationGroups.OrderByDescending(l => l.ConversionRate).ThenByDescending(l => l.Interactions).ToList()
        };

        var topLocation = locationGroups.FirstOrDefault();

        // Calculate delta for top location
        decimal topLocationDelta = 0;
        if (topLocation != null && topLocation.Location != "Unknown")
        {
            var prevEndDate = prevPeriod.End.AddDays(1);
            var prevEventTypes = InteractionEventTypes.ToList();

            var prevEvents = await context.CardAnalytics
                .Where(a => cardIds.Contains(a.CardId))
                .Where(a => a.Timestamp >= prevPeriod.Start && a.Timestamp < prevEndDate)
                .Where(a => prevEventTypes.Contains(a.EventType))
                .ToListAsync();

            var prevCount = prevEvents.Count(a => GetLocationKey(a.City, a.Country) == topLocation.Location);

            topLocationDelta = prevCount > 0 
                ? Math.Round((decimal)(topLocation.Interactions - prevCount) / prevCount * 100, 1) 
                : (topLocation.Interactions > 0 ? 100 : 0);
        }

        // Generate map points from locations with coords (with all metrics for tooltips)
        var mapPoints = locationGroups
            .Where(l => l.Latitude.HasValue && l.Longitude.HasValue)
            .Select(l => new MapPoint(
                l.Location,
                l.Latitude!.Value,
                l.Longitude!.Value,
                l.Interactions,
                l.Leads,
                l.ConversionRate,
                l.Location == topLocation?.Location ? topLocationDelta : 0,
                l.Location == topLocation?.Location
            ))
            .ToList();

        // Count locations pending geo resolution
        var pendingGeoCount = locationGroups.Count(l => !l.Latitude.HasValue && l.Location != "Unknown");

        // Calculate map center and zoom
        double? centerLat = null;
        double? centerLng = null;
        int defaultZoom = 2;

        if (mapPoints.Any())
        {
            centerLat = mapPoints.Average(p => p.Latitude);
            centerLng = mapPoints.Average(p => p.Longitude);
            
            // Calculate appropriate zoom based on spread
            var latSpread = mapPoints.Max(p => p.Latitude) - mapPoints.Min(p => p.Latitude);
            var lngSpread = mapPoints.Max(p => p.Longitude) - mapPoints.Min(p => p.Longitude);
            var maxSpread = Math.Max(latSpread, lngSpread);
            
            defaultZoom = maxSpread switch
            {
                > 100 => 2,
                > 50 => 3,
                > 20 => 4,
                > 10 => 5,
                > 5 => 6,
                > 2 => 7,
                _ => 8
            };
        }

        return new TopLocationsResult(
            topLocation?.Location ?? "No data",
            topLocation?.CountryCode ?? "",
            topLocationDelta,
            locationGroups.Any(),
            locationGroups.Take(5).ToList(),
            mapPoints,
            centerLat,
            centerLng,
            defaultZoom,
            pendingGeoCount
        );
    }

    /// <summary>
    /// Get coordinates for known locations (fallback when no geo data in event)
    /// </summary>
    private (double? Lat, double? Lng) GetKnownLocationCoords(string? city, string? countryCode)
    {
        if (string.IsNullOrEmpty(city)) return (null, null);
        
        var cityLower = city.ToLowerInvariant();
        var cc = countryCode?.ToUpperInvariant() ?? "";
        
        return (cityLower, cc) switch
        {
            // El Salvador
            ("san salvador", "SV") => (13.6929, -89.2182),
            ("santa ana", "SV") => (13.9942, -89.5597),
            ("san miguel", "SV") => (13.4833, -88.1833),
            ("la libertad", "SV") => (13.4883, -89.3225),
            
            // United States
            ("miami", "US") => (25.7617, -80.1918),
            ("boston", "US") => (42.3601, -71.0589),
            ("new york", "US") => (40.7128, -74.0060),
            ("los angeles", "US") => (34.0522, -118.2437),
            
            // Mexico
            ("ciudad de méxico", "MX") or ("cdmx", "MX") or ("mexico city", "MX") => (19.4326, -99.1332),
            ("guadalajara", "MX") => (20.6597, -103.3496),
            ("monterrey", "MX") => (25.6866, -100.3161),
            
            // Central America
            ("guatemala city", "GT") => (14.6349, -90.5069),
            ("tegucigalpa", "HN") => (14.0723, -87.1921),
            ("san josé", "CR") => (9.9281, -84.0907),
            
            // South America
            ("bogotá", "CO") => (4.7110, -74.0721),
            ("buenos aires", "AR") => (-34.6037, -58.3816),
            
            // Europe
            ("madrid", "ES") => (40.4168, -3.7038),
            ("london", "GB") => (51.5074, -0.1278),
            
            _ => (null, null)
        };
    }

    private string GetLocationKey(string? city, string? country)
    {
        if (string.IsNullOrEmpty(city) && string.IsNullOrEmpty(country))
            return "Unknown";
        
        if (string.IsNullOrEmpty(city))
            return country!;
        
        var countryCode = GetCountryCode(country);
        return string.IsNullOrEmpty(countryCode) ? city : $"{city}, {countryCode}";
    }

    private string GetCountryCode(string? country)
    {
        if (string.IsNullOrEmpty(country)) return "";
        
        return country.ToLower() switch
        {
            "el salvador" => "SV",
            "united states" or "usa" => "US",
            "mexico" or "méxico" => "MX",
            "spain" or "españa" => "ES",
            "argentina" => "AR",
            "colombia" => "CO",
            "guatemala" => "GT",
            "honduras" => "HN",
            "costa rica" => "CR",
            "panama" or "panamá" => "PA",
            _ => country.Length >= 2 ? country[..2].ToUpper() : country
        };
    }

    #endregion

    #region Top Links

    public async Task<List<LinkPerformance>> GetTopLinksAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        if (!cardIds.Any()) return new List<LinkPerformance>();
        
        var prevPeriod = dateRange.GetPreviousPeriod();

        // Get all CTA clicks in current period
        var endDate = dateRange.End.AddDays(1);
        
        var currentClicks = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
            .Where(a => a.EventType == "cta_click")
            .ToListAsync();

        if (!currentClicks.Any()) return new List<LinkPerformance>();

        // Get previous period clicks for trend
        var prevEndDate = prevPeriod.End.AddDays(1);

        var prevClicks = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= prevPeriod.Start && a.Timestamp < prevEndDate)
            .Where(a => a.EventType == "cta_click")
            .ToListAsync();

        // Get leads for conversion attribution (24h window)
        var startWindow = dateRange.Start.AddDays(-1);
        var endWindow = dateRange.End.AddDays(2);

        var leads = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= startWindow && l.CreatedAt < endWindow)
            .ToListAsync();

        // Group clicks by link type
        var linkGroups = currentClicks
            .GroupBy(c => ExtractLinkType(c))
            .Select(g =>
            {
                var clicks = g.Count();
                
                // Attribution: count leads created within 24h after a click
                var conversions = 0;
                foreach (var click in g)
                {
                    var attributionWindow = click.Timestamp.AddHours(24);
                    if (leads.Any(l => l.CardId == click.CardId && 
                                      l.CreatedAt >= click.Timestamp && 
                                      l.CreatedAt <= attributionWindow))
                    {
                        conversions++;
                    }
                }

                var hasConversion = clicks > 0;
                var conversionRate = hasConversion ? Math.Round((decimal)conversions / clicks * 100, 1) : 0;

                // Calculate trend vs previous period
                var prevCount = prevClicks.Count(c => ExtractLinkType(c) == g.Key);
                var trend = prevCount > 0 
                    ? Math.Round((decimal)(clicks - prevCount) / prevCount * 100, 1) 
                    : (clicks > 0 ? 100 : 0);

                return new LinkPerformance(
                    g.Key,
                    GetLinkIcon(g.Key),
                    clicks,
                    conversionRate,
                    trend,
                    hasConversion
                );
            })
            .OrderByDescending(l => l.Clicks)
            .Take(6)
            .ToList();

        return linkGroups;
    }

    private string ExtractLinkType(CardAnalytics analytics)
    {
        if (string.IsNullOrEmpty(analytics.MetadataJson))
        {
            return "Other";
        }

        try
        {
            var json = analytics.MetadataJson.ToLower();
            if (json.Contains("whatsapp")) return "WhatsApp";
            if (json.Contains("email")) return "Email";
            if (json.Contains("linkedin")) return "LinkedIn";
            if (json.Contains("instagram")) return "Instagram";
            if (json.Contains("portfolio")) return "Portfolio";
            if (json.Contains("website")) return "Website";
            if (json.Contains("call") || json.Contains("phone")) return "Call";
            if (json.Contains("calendar") || json.Contains("booking") || json.Contains("meeting")) return "Calendar";
            
            // Try to parse JSON for button field
            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(analytics.MetadataJson);
            if (metadata != null && metadata.TryGetValue("button", out var button))
            {
                return CapitalizeFirst(button);
            }
        }
        catch { }

        return "Other";
    }

    private string GetLinkIcon(string linkType)
    {
        return linkType.ToLower() switch
        {
            "whatsapp" => "💬",
            "email" => "📧",
            "linkedin" => "💼",
            "instagram" => "📷",
            "twitter" or "x" => "🐦",
            "portfolio" or "website" => "🌐",
            "call" or "phone" => "📞",
            "calendar" or "booking" or "meeting" => "📅",
            _ => "🔗"
        };
    }

    private string CapitalizeFirst(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input[1..].ToLower();
    }

    #endregion

    #region Insights

    public async Task<InsightsData> GetInsightsAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        var prevPeriod = dateRange.GetPreviousPeriod();

        // Top CTA calculation
        var topLinks = await GetTopLinksAsync(organizationId, dateRange);
        var topCta = topLinks.Where(l => l.Clicks > 0).OrderByDescending(l => l.ConversionRate).FirstOrDefault();
        var hasTopCta = topCta != null && topCta.Clicks > 0;

        // Previous period top CTA for delta
        decimal topCtaDelta = 0;
        if (hasTopCta)
        {
            var prevTopLinks = await GetTopLinksAsync(organizationId, prevPeriod);
            var prevTopCtaConversion = prevTopLinks
                .Where(l => l.LinkName == topCta!.LinkName)
                .Select(l => l.ConversionRate)
                .FirstOrDefault();

            topCtaDelta = prevTopCtaConversion > 0 
                ? Math.Round((topCta!.ConversionRate - prevTopCtaConversion) / prevTopCtaConversion * 100, 1) 
                : 0;
        }

        // Median Time to Lead
        var (medianTime, hasMedianTime) = await CalculateMedianTimeToLeadAsync(organizationId, dateRange);
        var (prevMedianTime, _) = await CalculateMedianTimeToLeadAsync(organizationId, prevPeriod);
        var medianDelta = prevMedianTime > 0 && hasMedianTime
            ? Math.Round((decimal)(medianTime - prevMedianTime) / prevMedianTime * 100, 1) 
            : 0;

        // Inactive Cards
        var totalCards = await context.Cards
            .Where(c => c.OrganizationId == organizationId && c.IsActive)
            .CountAsync();

        var endDate = dateRange.End.AddDays(1);

        var activeCardIds = cardIds.Any() 
            ? await context.CardAnalytics
                .Where(a => cardIds.Contains(a.CardId))
                .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
                .Select(a => a.CardId)
                .Distinct()
                .ToListAsync()
            : new List<Guid>();

        var inactiveCount = totalCards - activeCardIds.Count;
        var inactivePercent = totalCards > 0 ? Math.Round((decimal)inactiveCount / totalCards * 100, 1) : 0;

        // Potential loss calculation
        var potentialLossEndDate = dateRange.End.AddDays(1);

        var totalInteractions = cardIds.Any() 
            ? await context.CardAnalytics
                .Where(a => activeCardIds.Contains(a.CardId))
                .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < potentialLossEndDate)
                .CountAsync()
            : 0;
            
        var avgInteractionsPerActiveCard = activeCardIds.Count > 0
            ? totalInteractions / activeCardIds.Count
            : 0;

        var potentialLoss = inactiveCount * avgInteractionsPerActiveCard;

        return new InsightsData(
            topCta?.LinkName ?? "N/A",
            topCta?.ConversionRate ?? 0,
            topCtaDelta,
            hasTopCta,
            medianTime,
            medianDelta,
            hasMedianTime,
            inactiveCount,
            inactivePercent,
            potentialLoss,
            totalCards
        );
    }

    private async Task<(int median, bool hasData)> CalculateMedianTimeToLeadAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        if (!cardIds.Any()) return (0, false);

        // Get leads in range
        var endDate = dateRange.End.AddDays(1);

        var leads = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= dateRange.Start && l.CreatedAt < endDate)
            .ToListAsync();

        if (!leads.Any()) return (0, false);

        var timesToLead = new List<int>();

        foreach (var lead in leads)
        {
            // Find first interaction for this card before lead creation
            var eventTypes = InteractionEventTypes.ToList();
            var firstInteraction = await context.CardAnalytics
                .Where(a => a.CardId == lead.CardId)
                .Where(a => a.Timestamp <= lead.CreatedAt)
                .Where(a => eventTypes.Contains(a.EventType))
                .OrderBy(a => a.Timestamp)
                .Select(a => a.Timestamp)
                .FirstOrDefaultAsync();

            if (firstInteraction != default)
            {
                var minutes = (int)(lead.CreatedAt - firstInteraction).TotalMinutes;
                if (minutes >= 0 && minutes < 10080) // Max 1 week
                {
                    timesToLead.Add(minutes);
                }
            }
        }

        if (!timesToLead.Any()) return (0, false);

        timesToLead.Sort();
        var mid = timesToLead.Count / 2;
        var median = timesToLead.Count % 2 == 0 
            ? (timesToLead[mid - 1] + timesToLead[mid]) / 2 
            : timesToLead[mid];
            
        return (median, true);
    }

    #endregion

    #region High-Intent

    public async Task<HighIntentData> GetHighIntentDataAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);
        if (!cardIds.Any())
        {
            return new HighIntentData(0, 0, 0, 0, 0, 0, new List<RecentActivity>());
        }

        // Get all relevant events
        var endDate = dateRange.End.AddDays(1);
        var highIntentInfos = HighIntentEventTypes.ToList();

        var allEvents = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
            .Where(a => highIntentInfos.Contains(a.EventType) || a.EventType == "cta_click")
            .ToListAsync();

        // Categorize events
        var contactsSaved = allEvents.Count(e => e.EventType == "contact_save");
        var formSubmits = allEvents.Count(e => e.EventType == "form_submit");
        
        var ctaClicks = allEvents.Where(e => e.EventType == "cta_click" && e.MetadataJson != null).ToList();
        
        var whatsAppClicks = ctaClicks.Count(e => 
            e.MetadataJson!.Contains("whatsapp", StringComparison.OrdinalIgnoreCase));
        var callClicks = ctaClicks.Count(e => 
            e.MetadataJson!.Contains("call", StringComparison.OrdinalIgnoreCase) ||
            e.MetadataJson!.Contains("phone", StringComparison.OrdinalIgnoreCase));
        var meetingsBooked = ctaClicks.Count(e => 
            e.MetadataJson!.Contains("calendar", StringComparison.OrdinalIgnoreCase) ||
            e.MetadataJson!.Contains("booking", StringComparison.OrdinalIgnoreCase) ||
            e.MetadataJson!.Contains("meeting", StringComparison.OrdinalIgnoreCase));

        var totalHighIntent = contactsSaved + formSubmits + whatsAppClicks + callClicks + meetingsBooked;

        // Get recent activities (last 5 high-intent events)
        var recentHighIntentEvents = allEvents
            .Where(e => e.EventType == "contact_save" || 
                       e.EventType == "form_submit" ||
                       (e.EventType == "cta_click" && e.MetadataJson != null && 
                        HighIntentCtaButtons.Any(b => e.MetadataJson.Contains(b, StringComparison.OrdinalIgnoreCase))))
            .OrderByDescending(e => e.Timestamp)
            .Take(10)
            .ToList();

        // Get lead names for matching
        var recentLeads = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(20)
            .Select(l => new { l.Id, l.FullName, l.CreatedAt, l.CardId })
            .ToListAsync();

        var recentActivities = new List<RecentActivity>();

        foreach (var evt in recentHighIntentEvents.Take(5))
        {
            var lead = recentLeads.FirstOrDefault(l => 
                l.CardId == evt.CardId && 
                Math.Abs((l.CreatedAt - evt.Timestamp).TotalMinutes) < 30);

            var timeAgo = GetTimeAgo(evt.Timestamp);
            var description = GetEventDescription(evt);

            recentActivities.Add(new RecentActivity(
                description,
                timeAgo,
                lead?.Id,
                evt.EventType,
                lead?.FullName ?? GetContactFromMetadata(evt.MetadataJson)
            ));
        }

        return new HighIntentData(
            totalHighIntent,
            contactsSaved,
            meetingsBooked,
            whatsAppClicks,
            callClicks,
            formSubmits,
            recentActivities
        );
    }

    private string GetEventDescription(CardAnalytics evt)
    {
        if (evt.EventType == "contact_save") return "saved contact";
        if (evt.EventType == "form_submit") return "submitted form";
        if (evt.EventType == "cta_click" && evt.MetadataJson != null)
        {
            var json = evt.MetadataJson.ToLower();
            if (json.Contains("whatsapp")) return "clicked WhatsApp";
            if (json.Contains("call") || json.Contains("phone")) return "clicked Call";
            if (json.Contains("calendar") || json.Contains("booking") || json.Contains("meeting")) return "booked meeting";
        }
        return "interaction";
    }

    private string? GetContactFromMetadata(string? metadataJson)
    {
        if (string.IsNullOrEmpty(metadataJson)) return null;
        try
        {
            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
            if (metadata != null)
            {
                if (metadata.TryGetValue("name", out var name)) return name;
                if (metadata.TryGetValue("contact_name", out var contactName)) return contactName;
            }
        }
        catch { }
        return null;
    }

    private string GetTimeAgo(DateTime timestamp)
    {
        var span = DateTime.UtcNow - timestamp;
        
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
        return timestamp.ToString("MMM dd");
    }

    #endregion

    #region Helpers

    private static async Task<List<Guid>> GetOrganizationCardIdsInternal(DataTouchDbContext context, Guid organizationId)
    {
        return await context.Cards
            .Where(c => c.OrganizationId == organizationId)
            .Select(c => c.Id)
            .ToListAsync();
    }

    public static DateRangeFilter GetDateRangeFromOption(string option)
    {
        var end = DateTime.UtcNow.Date;
        var start = option switch
        {
            "7d" => end.AddDays(-6),
            "14d" => end.AddDays(-13),
            "30d" => end.AddDays(-29),
            "90d" => end.AddDays(-89),
            _ => end.AddDays(-6)
        };
        return new DateRangeFilter(start, end);
    }

    #endregion
}
