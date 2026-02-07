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
    
    /// <summary>
    /// Data point aggregated by weekday (always 7 fixed buckets: L M M J V S D)
    /// </summary>
    public record WeekdayChartDataPoint(
        DayOfWeek DayOfWeek,
        string DayLabel,       // "L", "M", "M", "J", "V", "S", "D"
        string DayName,        // "Lunes", "Martes", etc.
        int Interactions,
        int Leads,
        int OccurrenceCount    // How many times this weekday appears in the range
    );

    /// <summary>
    /// Enterprise grouping types for chart visualization
    /// </summary>
    public enum GroupingType { Hours, Days, Weeks, Months, Quarters }

    /// <summary>
    /// Single bucket/bar in the grouped chart
    /// </summary>
    public record ChartBucket(
        string Label,           // "L", "Sem 1", "Feb", "T1", "08h"
        string TooltipTitle,    // "Lunes", "Semana 1", "Febrero", "T1"
        string TooltipRange,    // "08 ene – 14 ene", "01 feb – 03 feb"
        int Interactions,
        int Leads,
        decimal ConversionRate
    );

    /// <summary>
    /// Complete grouped chart data with range info and buckets
    /// </summary>
    public record GroupedChartData(
        string RangeLabel,      // "28 ene – 03 feb", "sep 2025 – feb 2026"
        string GroupingUnit,    // "Horas", "Días", "Semanas", "Meses", "Trimestres"
        GroupingType Grouping,
        List<ChartBucket> Buckets
    );
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

    public enum ChartAggregation { Day, Week, Month, Total }
    public enum LocationSortBy { Conversion, Leads, Interactions }

    // ───── Resumen Inteligente (Smart Summary) ─────

    public enum DataQuality { Alta, Media, Baja }

    public record SmartSummaryMetrics(
        int V, int AC, int IC, int CC, int AE, int AT,
        decimal TC, decimal TI, DataQuality Quality,
        int PrevV, int PrevAC, int PrevAE, int PrevAT);

    public record SmartSummaryCard(
        string Chip, string ChipColor, string Title,
        string Description, string BaseLine,
        string? CtaText, string? CtaRoute, string ScenarioKey);

    public record ChannelBreakdown(
        string ChannelName, string ChannelType, int Count, decimal Percent);

    public record SmartSummaryData(
        SmartSummaryMetrics Metrics,
        SmartSummaryCard Rendimiento, SmartSummaryCard CanalPrincipal,
        SmartSummaryCard Friccion, SmartSummaryCard ProximoPaso,
        List<ChannelBreakdown> TopChannels,
        List<ChannelBreakdown> AllChannels,
        bool HasData);

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

        // Handle "Total" aggregation - single data point
        if (aggregation == ChartAggregation.Total)
        {
            var totalInteractions = interactions.Count;
            var totalLeads = leads.Count;
            return new List<ChartDataPoint> { new ChartDataPoint(dateRange.Start, totalInteractions, totalLeads) };
        }

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

    /// <summary>
    /// Get chart data aggregated by weekday (always 7 fixed points: L M M J V S D).
    /// The date range only determines which data to include in the aggregation.
    /// </summary>
    public async Task<List<WeekdayChartDataPoint>> GetWeekdayAggregatedChartAsync(
        Guid organizationId, 
        DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);

        // Get all interactions in range
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

        // Get all leads in range
        var leads = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= dateRange.Start && l.CreatedAt < endDate)
            .Select(l => l.CreatedAt)
            .ToListAsync();

        // Count occurrences of each weekday in the date range
        var weekdayOccurrences = new Dictionary<DayOfWeek, int>();
        for (var day = dateRange.Start.Date; day <= dateRange.End.Date; day = day.AddDays(1))
        {
            if (!weekdayOccurrences.ContainsKey(day.DayOfWeek))
                weekdayOccurrences[day.DayOfWeek] = 0;
            weekdayOccurrences[day.DayOfWeek]++;
        }

        // Spanish weekday labels and names
        var weekdayData = new Dictionary<DayOfWeek, (string Label, string Name)>
        {
            { DayOfWeek.Monday, ("L", "Lunes") },
            { DayOfWeek.Tuesday, ("M", "Martes") },
            { DayOfWeek.Wednesday, ("M", "Miércoles") },
            { DayOfWeek.Thursday, ("J", "Jueves") },
            { DayOfWeek.Friday, ("V", "Viernes") },
            { DayOfWeek.Saturday, ("S", "Sábado") },
            { DayOfWeek.Sunday, ("D", "Domingo") }
        };

        // Create exactly 7 buckets, one per weekday
        var weekdayOrder = new[] 
        { 
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday 
        };

        var result = new List<WeekdayChartDataPoint>();

        foreach (var dayOfWeek in weekdayOrder)
        {
            var (label, name) = weekdayData[dayOfWeek];
            var interactionCount = interactions.Count(i => i.DayOfWeek == dayOfWeek);
            var leadCount = leads.Count(l => l.DayOfWeek == dayOfWeek);
            var occurrences = weekdayOccurrences.GetValueOrDefault(dayOfWeek, 0);

            result.Add(new WeekdayChartDataPoint(
                dayOfWeek,
                label,
                name,
                interactionCount,
                leadCount,
                occurrences
            ));
        }

        return result;
    }

    /// <summary>
    /// Get chart data with enterprise grouping based on date range.
    /// Grouping rules:
    /// - Today: Hours (6-8 buckets)
    /// - 7 days: Days (L M M J V S D)
    /// - 15 days: Weeks (Sem 1-3)
    /// - 30 days: Weeks (Sem 1-5)
    /// - 6 months: Months (6 buckets)
    /// - 12 months: Quarters (T1-T4)
    /// - Custom: Auto based on duration
    /// </summary>
    public async Task<GroupedChartData> GetGroupedChartDataAsync(
        Guid organizationId,
        DateRangeFilter dateRange,
        string filterOption)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);

        var endDate = dateRange.End.AddDays(1);
        var eventTypes = InteractionEventTypes.ToList();

        // Fetch all interactions and leads in range
        var interactions = cardIds.Any()
            ? await context.CardAnalytics
                .Where(a => cardIds.Contains(a.CardId))
                .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
                .Where(a => eventTypes.Contains(a.EventType))
                .Select(a => a.Timestamp)
                .ToListAsync()
            : new List<DateTime>();

        var leads = await context.Leads
            .Where(l => l.OrganizationId == organizationId)
            .Where(l => l.CreatedAt >= dateRange.Start && l.CreatedAt < endDate)
            .Select(l => l.CreatedAt)
            .ToListAsync();

        // Determine grouping based on filter
        var (grouping, buckets) = CreateBuckets(dateRange, filterOption, interactions, leads);
        var rangeLabel = FormatRangeLabel(dateRange, grouping);
        var groupingUnit = GetGroupingUnitName(grouping);

        return new GroupedChartData(rangeLabel, groupingUnit, grouping, buckets);
    }

    private (GroupingType, List<ChartBucket>) CreateBuckets(
        DateRangeFilter dateRange,
        string filterOption,
        List<DateTime> interactions,
        List<DateTime> leads)
    {
        var days = (dateRange.End - dateRange.Start).Days + 1;

        // Determine grouping type
        GroupingType grouping = filterOption switch
        {
            "today" => GroupingType.Hours,
            "7d" => GroupingType.Days,
            "15d" => GroupingType.Weeks,
            "30d" => GroupingType.Weeks,
            "6m" => GroupingType.Months,
            "12m" => GroupingType.Quarters,
            _ => days <= 2 ? GroupingType.Hours
                : days <= 14 ? GroupingType.Days
                : days <= 60 ? GroupingType.Weeks
                : days <= 365 ? GroupingType.Months
                : GroupingType.Quarters
        };

        return grouping switch
        {
            GroupingType.Hours => (grouping, CreateHourBuckets(dateRange, interactions, leads)),
            GroupingType.Days => (grouping, CreateDayBuckets(dateRange, interactions, leads)),
            GroupingType.Weeks => (grouping, CreateWeekBuckets(dateRange, interactions, leads)),
            GroupingType.Months => (grouping, CreateMonthBuckets(dateRange, interactions, leads)),
            GroupingType.Quarters => (grouping, CreateQuarterBuckets(dateRange, interactions, leads)),
            _ => (grouping, CreateDayBuckets(dateRange, interactions, leads))
        };
    }

    private List<ChartBucket> CreateHourBuckets(DateRangeFilter range, List<DateTime> interactions, List<DateTime> leads)
    {
        var buckets = new List<ChartBucket>();
        var hours = new[] { 0, 4, 8, 12, 16, 20 }; // 6 buckets of 4 hours each

        foreach (var startHour in hours)
        {
            var endHour = startHour + 4;
            var intCount = interactions.Count(i => i.Hour >= startHour && i.Hour < endHour);
            var leadCount = leads.Count(l => l.Hour >= startHour && l.Hour < endHour);
            var conv = intCount > 0 ? (decimal)leadCount / intCount * 100 : 0;

            buckets.Add(new ChartBucket(
                Label: $"{startHour:D2}h",
                TooltipTitle: $"{startHour:D2}:00 – {endHour - 1:D2}:59",
                TooltipRange: range.Start.ToString("dd MMM yyyy", new System.Globalization.CultureInfo("es-ES")),
                Interactions: intCount,
                Leads: leadCount,
                ConversionRate: conv
            ));
        }

        return buckets;
    }

    private List<ChartBucket> CreateDayBuckets(DateRangeFilter range, List<DateTime> interactions, List<DateTime> leads)
    {
        var buckets = new List<ChartBucket>();
        var dayLabels = new Dictionary<DayOfWeek, (string Label, string Name)>
        {
            { DayOfWeek.Monday, ("L", "Lunes") },
            { DayOfWeek.Tuesday, ("M", "Martes") },
            { DayOfWeek.Wednesday, ("M", "Miércoles") },
            { DayOfWeek.Thursday, ("J", "Jueves") },
            { DayOfWeek.Friday, ("V", "Viernes") },
            { DayOfWeek.Saturday, ("S", "Sábado") },
            { DayOfWeek.Sunday, ("D", "Domingo") }
        };

        var weekdayOrder = new[] 
        { 
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, 
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday 
        };

        foreach (var dayOfWeek in weekdayOrder)
        {
            var (label, name) = dayLabels[dayOfWeek];
            var intCount = interactions.Count(i => i.DayOfWeek == dayOfWeek);
            var leadCount = leads.Count(l => l.DayOfWeek == dayOfWeek);
            var conv = intCount > 0 ? (decimal)leadCount / intCount * 100 : 0;

            // Find dates for tooltip
            var dates = Enumerable.Range(0, range.DayCount)
                .Select(d => range.Start.AddDays(d))
                .Where(d => d.DayOfWeek == dayOfWeek)
                .ToList();
            var tooltipRange = dates.Count == 1
                ? dates[0].ToString("dd MMM", new System.Globalization.CultureInfo("es-ES"))
                : $"{dates.Count} {name.ToLower()}s";

            buckets.Add(new ChartBucket(
                Label: label,
                TooltipTitle: name,
                TooltipRange: tooltipRange,
                Interactions: intCount,
                Leads: leadCount,
                ConversionRate: conv
            ));
        }

        return buckets;
    }

    private List<ChartBucket> CreateWeekBuckets(DateRangeFilter range, List<DateTime> interactions, List<DateTime> leads)
    {
        var buckets = new List<ChartBucket>();
        var weekNum = 1;
        var weekStart = range.Start;
        var culture = new System.Globalization.CultureInfo("es-ES");

        while (weekStart <= range.End)
        {
            var weekEnd = weekStart.AddDays(6);
            if (weekEnd > range.End) weekEnd = range.End;

            var intCount = interactions.Count(i => i.Date >= weekStart && i.Date <= weekEnd);
            var leadCount = leads.Count(l => l.Date >= weekStart && l.Date <= weekEnd);
            var conv = intCount > 0 ? (decimal)leadCount / intCount * 100 : 0;

            buckets.Add(new ChartBucket(
                Label: $"Sem {weekNum}",
                TooltipTitle: $"Semana {weekNum}",
                TooltipRange: $"{weekStart:dd} – {weekEnd:dd MMM}",
                Interactions: intCount,
                Leads: leadCount,
                ConversionRate: conv
            ));

            weekNum++;
            weekStart = weekEnd.AddDays(1);
        }

        return buckets;
    }

    private List<ChartBucket> CreateMonthBuckets(DateRangeFilter range, List<DateTime> interactions, List<DateTime> leads)
    {
        var buckets = new List<ChartBucket>();
        var monthNames = new[] { "", "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
        var monthFullNames = new[] { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

        var current = new DateTime(range.Start.Year, range.Start.Month, 1);
        var endMonth = new DateTime(range.End.Year, range.End.Month, 1);

        while (current <= endMonth)
        {
            var monthStart = current;
            var monthEnd = current.AddMonths(1).AddDays(-1);
            if (monthStart < range.Start) monthStart = range.Start;
            if (monthEnd > range.End) monthEnd = range.End;

            var intCount = interactions.Count(i => i.Date >= monthStart && i.Date <= monthEnd);
            var leadCount = leads.Count(l => l.Date >= monthStart && l.Date <= monthEnd);
            var conv = intCount > 0 ? (decimal)leadCount / intCount * 100 : 0;

            var label = monthNames[current.Month];
            var yearSuffix = current.Year != DateTime.Now.Year ? $" {current.Year % 100}" : "";

            buckets.Add(new ChartBucket(
                Label: $"{label}{yearSuffix}",
                TooltipTitle: $"{monthFullNames[current.Month]} {current.Year}",
                TooltipRange: $"{monthStart:dd} – {monthEnd:dd MMM}",
                Interactions: intCount,
                Leads: leadCount,
                ConversionRate: conv
            ));

            current = current.AddMonths(1);
        }

        return buckets;
    }

    private List<ChartBucket> CreateQuarterBuckets(DateRangeFilter range, List<DateTime> interactions, List<DateTime> leads)
    {
        var buckets = new List<ChartBucket>();
        var quarterMonths = new Dictionary<int, (int StartMonth, int EndMonth, string Name)>
        {
            { 1, (1, 3, "Ene – Mar") },
            { 2, (4, 6, "Abr – Jun") },
            { 3, (7, 9, "Jul – Sep") },
            { 4, (10, 12, "Oct – Dic") }
        };

        var startQuarter = (range.Start.Month - 1) / 3 + 1;
        var startYear = range.Start.Year;
        var endQuarter = (range.End.Month - 1) / 3 + 1;
        var endYear = range.End.Year;

        var currentYear = startYear;
        var currentQuarter = startQuarter;

        while (currentYear < endYear || (currentYear == endYear && currentQuarter <= endQuarter))
        {
            var (startMonth, endMonth, name) = quarterMonths[currentQuarter];
            var qStart = new DateTime(currentYear, startMonth, 1);
            var qEnd = new DateTime(currentYear, endMonth, DateTime.DaysInMonth(currentYear, endMonth));

            if (qStart < range.Start) qStart = range.Start;
            if (qEnd > range.End) qEnd = range.End;

            var intCount = interactions.Count(i => i.Date >= qStart && i.Date <= qEnd);
            var leadCount = leads.Count(l => l.Date >= qStart && l.Date <= qEnd);
            var conv = intCount > 0 ? (decimal)leadCount / intCount * 100 : 0;

            buckets.Add(new ChartBucket(
                Label: $"T{currentQuarter}",
                TooltipTitle: $"T{currentQuarter} {currentYear}",
                TooltipRange: name,
                Interactions: intCount,
                Leads: leadCount,
                ConversionRate: conv
            ));

            currentQuarter++;
            if (currentQuarter > 4)
            {
                currentQuarter = 1;
                currentYear++;
            }
        }

        return buckets;
    }

    private string FormatRangeLabel(DateRangeFilter range, GroupingType grouping)
    {
        var culture = new System.Globalization.CultureInfo("es-ES");

        if (range.DayCount == 1)
            return "Hoy";

        if (grouping == GroupingType.Months || grouping == GroupingType.Quarters)
        {
            var startMonth = range.Start.ToString("MMM yyyy", culture).ToLower();
            var endMonth = range.End.ToString("MMM yyyy", culture).ToLower();
            return $"{startMonth} – {endMonth}";
        }

        var start = range.Start.ToString("dd MMM", culture);
        var end = range.End.ToString("dd MMM", culture);
        return $"{start} – {end}";
    }

    private string GetGroupingUnitName(GroupingType grouping) => grouping switch
    {
        GroupingType.Hours => "Horas",
        GroupingType.Days => "Días",
        GroupingType.Weeks => "Semanas",
        GroupingType.Months => "Meses",
        GroupingType.Quarters => "Trimestres",
        _ => "Días"
    };

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
            "today" => end,
            "7d" => end.AddDays(-6),
            "14d" => end.AddDays(-13),
            "15d" => end.AddDays(-14),
            "30d" => end.AddDays(-29),
            "90d" => end.AddDays(-89),
            "6m" => end.AddMonths(-6),
            "12m" => end.AddMonths(-12),
            _ => end.AddDays(-14) // Default to 15 days
        };
        return new DateRangeFilter(start, end);
    }

    public static DateRangeFilter GetCustomDateRange(DateTime customStart, DateTime customEnd)
    {
        return new DateRangeFilter(customStart.Date, customEnd.Date);
    }

    #endregion

    #region Smart Summary

    public async Task<SmartSummaryData> GetSmartSummaryAsync(Guid organizationId, DateRangeFilter dateRange)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var cardIds = await GetOrganizationCardIdsInternal(context, organizationId);

        if (!cardIds.Any())
            return EmptySmartSummary();

        var prevPeriod = dateRange.GetPreviousPeriod();
        var endDate = dateRange.End.AddDays(1);
        var prevEndDate = prevPeriod.End.AddDays(1);

        // Single query: all events in current period
        var currentEvents = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= dateRange.Start && a.Timestamp < endDate)
            .ToListAsync();

        // Single query: all events in previous period
        var prevEvents = await context.CardAnalytics
            .Where(a => cardIds.Contains(a.CardId))
            .Where(a => a.Timestamp >= prevPeriod.Start && a.Timestamp < prevEndDate)
            .ToListAsync();

        if (!currentEvents.Any())
            return EmptySmartSummary();

        var metrics = CalculateSmartMetrics(currentEvents, prevEvents);
        var channelBreakdown = BuildChannelBreakdown(currentEvents);

        var rendimiento = DetermineRendimiento(metrics);
        var canalPrincipal = DetermineCanalPrincipal(metrics, channelBreakdown);
        var friccion = DetermineFriccion(metrics, currentEvents);
        var proximoPaso = DetermineProximoPaso(friccion.ScenarioKey, metrics);

        // Build TopChannels: top 2 + "Otros" aggregate
        var top2 = channelBreakdown.Take(2).ToList();
        var otrosChannels = channelBreakdown.Skip(2).ToList();
        var topChannelsForCard = new List<ChannelBreakdown>(top2);

        if (otrosChannels.Any())
        {
            var otrosCount = otrosChannels.Sum(c => c.Count);
            var otrosPercent = otrosChannels.Sum(c => c.Percent);
            topChannelsForCard.Add(new ChannelBreakdown("Otros", "mixed", otrosCount, Math.Round(otrosPercent, 1)));
        }

        return new SmartSummaryData(
            metrics, rendimiento, canalPrincipal, friccion, proximoPaso,
            topChannelsForCard,
            channelBreakdown,
            HasData: true);
    }

    // ───── Metric Calculation ─────

    private static SmartSummaryMetrics CalculateSmartMetrics(
        List<CardAnalytics> current, List<CardAnalytics> previous)
    {
        // Current period
        int V = current.Count(e => e.EventType == "page_view");

        int whatsapp = CountCtaByButton(current, "whatsapp");
        int call = CountCtaByButton(current, "call", "phone");
        int email = CountCtaByButton(current, "email");
        int contactSave = current.Count(e => e.EventType == "contact_save");
        int formSubmit = current.Count(e => e.EventType == "form_submit");

        int IC = whatsapp + call + email + contactSave; // Intents
        int CC = formSubmit; // Confirmed contacts (only completions)
        int AC = IC + CC;

        int socialClicks = CountCtaByButton(current, "linkedin", "instagram", "facebook", "youtube", "twitter", "x");
        int webClicks = CountCtaByButton(current, "website", "portfolio");
        int AE = socialClicks + webClicks;
        int AT = AC + AE;

        decimal TC = V > 0 ? Math.Round((decimal)CC / V * 100, 2) : 0;
        decimal TI = V > 0 ? Math.Round((decimal)IC / V * 100, 2) : 0;

        DataQuality quality;
        if (V >= 200 || AT >= 80) quality = DataQuality.Alta;
        else if (V >= 80 || AT >= 30) quality = DataQuality.Media;
        else quality = DataQuality.Baja;

        // Previous period
        int prevV = previous.Count(e => e.EventType == "page_view");
        int prevWhatsapp = CountCtaByButton(previous, "whatsapp");
        int prevCall = CountCtaByButton(previous, "call", "phone");
        int prevEmail = CountCtaByButton(previous, "email");
        int prevContactSave = previous.Count(e => e.EventType == "contact_save");
        int prevFormSubmit = previous.Count(e => e.EventType == "form_submit");
        int prevIC = prevWhatsapp + prevCall + prevEmail + prevContactSave;
        int prevCC = prevFormSubmit;
        int prevAC = prevIC + prevCC;
        int prevSocial = CountCtaByButton(previous, "linkedin", "instagram", "facebook", "youtube", "twitter", "x");
        int prevWeb = CountCtaByButton(previous, "website", "portfolio");
        int prevAE = prevSocial + prevWeb;
        int prevAT = prevAC + prevAE;

        return new SmartSummaryMetrics(V, AC, IC, CC, AE, AT, TC, TI, quality,
            prevV, prevAC, prevAE, prevAT);
    }

    private static int CountCtaByButton(List<CardAnalytics> events, params string[] buttons)
    {
        return events.Count(e =>
            e.EventType == "cta_click" &&
            e.MetadataJson != null &&
            buttons.Any(b => e.MetadataJson.Contains(b, StringComparison.OrdinalIgnoreCase)));
    }

    // ───── Channel Breakdown ─────

    private static List<ChannelBreakdown> BuildChannelBreakdown(List<CardAnalytics> events)
    {
        var channelDefs = new (string Name, string Type, string[] Keywords)[]
        {
            ("WhatsApp", "contact", new[] { "whatsapp" }),
            ("Llamada", "contact", new[] { "call", "phone" }),
            ("Email", "contact", new[] { "email" }),
            ("LinkedIn", "exploration", new[] { "linkedin" }),
            ("Instagram", "exploration", new[] { "instagram" }),
            ("Facebook", "exploration", new[] { "facebook" }),
            ("YouTube", "exploration", new[] { "youtube" }),
            ("X/Twitter", "exploration", new[] { "twitter", "x" }),
            ("Website", "exploration", new[] { "website" }),
            ("Portfolio", "exploration", new[] { "portfolio" }),
        };

        var ctaEvents = events.Where(e => e.EventType == "cta_click" && e.MetadataJson != null).ToList();
        var results = new List<ChannelBreakdown>();

        foreach (var (name, type, keywords) in channelDefs)
        {
            var count = ctaEvents.Count(e =>
                keywords.Any(k => e.MetadataJson!.Contains(k, StringComparison.OrdinalIgnoreCase)));
            if (count > 0)
                results.Add(new ChannelBreakdown(name, type, count, 0));
        }

        // contact_save and form_submit as channels
        var contactSaves = events.Count(e => e.EventType == "contact_save");
        if (contactSaves > 0)
            results.Add(new ChannelBreakdown("Guardar Contacto", "contact", contactSaves, 0));

        var formSubmits = events.Count(e => e.EventType == "form_submit");
        if (formSubmits > 0)
            results.Add(new ChannelBreakdown("Formulario", "contact", formSubmits, 0));

        var total = results.Sum(r => r.Count);

        return results
            .Select(r => r with { Percent = total > 0 ? Math.Round((decimal)r.Count / total * 100, 1) : 0 })
            .OrderByDescending(r => r.Count)
            .ToList();
    }

    // ───── Scenario Determination ─────

    private static SmartSummaryCard DetermineRendimiento(SmartSummaryMetrics m)
    {
        // P1: Quality Baja
        if (m.Quality == DataQuality.Baja)
            return MakeCard("RENDIMIENTO", "chip-perf",
                "Aún hay poca información",
                $"Con {m.V} visitas y {m.AT} acciones, aún no hay suficiente volumen para un diagnóstico confiable.",
                $"Base: {m.V} visitas · {m.AT} acciones totales", "Compartir Tarjeta", "/cards/mine", "low-data");

        // P2: Strong decline (V or AC drop >= 30%)
        decimal vDelta = m.PrevV > 0 ? (decimal)(m.V - m.PrevV) / m.PrevV * 100 : 0;
        decimal acDelta = m.PrevAC > 0 ? (decimal)(m.AC - m.PrevAC) / m.PrevAC * 100 : 0;
        if (vDelta <= -30 || acDelta <= -30)
            return MakeCard("RENDIMIENTO", "chip-perf",
                "Rendimiento en baja",
                $"Las visitas o acciones cayeron significativamente ({FormatDelta(Math.Min(vDelta, acDelta))}) respecto al período anterior.",
                $"{m.V} visitas ({FormatDelta(vDelta)}) · {m.AC} acciones ({FormatDelta(acDelta)})",
                "Ver Detalle", "/cards/mine", "decline");

        // P3: Exploration dominates (AE >= 55% of AT and CC very low)
        decimal aePercent = m.AT > 0 ? (decimal)m.AE / m.AT * 100 : 0;
        if (aePercent >= 55 && m.CC <= 2)
            return MakeCard("RENDIMIENTO", "chip-perf",
                "Se quedan explorando",
                $"El {aePercent:F0}% de las acciones son exploración (redes/web) pero casi nadie deja datos de contacto.",
                $"{m.AE} exploraciones ({aePercent:F0}%) · {m.CC} contactos confirmados", "Ver Detalle", "/cards/mine", "exploring");

        // P4: Interest without contact (AC high but CC low)
        if (m.AC > 10 && m.CC <= 2)
            return MakeCard("RENDIMIENTO", "chip-perf",
                "Hay interés, pero no contactan",
                $"Se registraron {m.AC} acciones de contacto pero solo {m.CC} confirmaron. La intención existe pero algo frena la conversión.",
                $"{m.AC} acciones · {m.CC} contactos · {m.TC:F1}% conversión", "Ver Detalle", "/cards/mine", "interest-no-contact");

        // P5: Good performance (default)
        return MakeCard("RENDIMIENTO", "chip-perf",
            "Buen rendimiento",
            $"Tu tarjeta funciona bien con {m.V} visitas, {m.AC} acciones de contacto y una tasa de {m.TC:F1}%.",
            $"Base: {m.V} visitas · {m.AC} acciones · {m.CC} contactos", "Ver Detalle", "/cards/mine", "good");
    }

    private static SmartSummaryCard DetermineCanalPrincipal(
        SmartSummaryMetrics m, List<ChannelBreakdown> channels)
    {
        if (!channels.Any() || m.AT == 0)
            return MakeCard("CANAL PRINCIPAL", "chip-channel",
                "Sin datos de canal",
                "No hay suficientes acciones para identificar un canal dominante.",
                "Sin acciones registradas", null, null, "no-channel");

        var top1 = channels[0];
        var top2 = channels.Count > 1 ? channels[1] : null;

        // Healthy distribution
        if (top1.Percent < 45 && top2 != null && (top1.Percent - top2.Percent) < 15)
            return MakeCard("CANAL PRINCIPAL", "chip-channel",
                "Distribución saludable",
                $"No hay un canal dominante. {top1.ChannelName} ({top1.Percent:F0}%) y {top2.ChannelName} ({top2.Percent:F0}%) están equilibrados.",
                $"{top1.ChannelName}: {top1.Count} clics · {top2.ChannelName}: {top2.Count} clics",
                null, null, "balanced");

        // Single dominant channel
        string typeLabel = top1.ChannelType == "contact" ? "Contacto" : "Exploración";
        return MakeCard("CANAL PRINCIPAL", "chip-channel",
            top1.ChannelName,
            $"Es tu canal principal ({top1.Percent:F0}% del total de acciones). Tipo: {typeLabel}.",
            $"{top1.ChannelName}: {top1.Count} clics ({top1.Percent:F0}%)",
            null, null, "dominant");
    }

    private static SmartSummaryCard DetermineFriccion(
        SmartSummaryMetrics m, List<CardAnalytics> events)
    {
        // P1: Quality Baja
        if (m.Quality == DataQuality.Baja)
            return MakeCard("FRICCIÓN", "chip-friction",
                "No hay suficiente data",
                "Se necesitan más interacciones para detectar puntos de fricción.",
                $"{m.V} visitas · {m.AT} acciones totales", null, null, "no-data");

        // P2: Form abandonment (future-proof — form_start not tracked yet)
        int formStart = 0; // Will be > 0 once tracked
        int formSubmit = events.Count(e => e.EventType == "form_submit");
        if (formStart > 0)
        {
            decimal abandonRate = 1m - ((decimal)formSubmit / formStart);
            if (abandonRate >= 0.40m)
                return MakeCard("FRICCIÓN", "chip-friction",
                    "El formulario se abandona",
                    $"El {abandonRate * 100:F0}% de quienes abren el formulario no lo completan.",
                    $"Iniciados: {formStart} · Enviados: {formSubmit}",
                    "Editar Formulario", "/cards/mine", "form-abandon");
        }

        // P3: High exploration, no contact
        decimal aePercent = m.AT > 0 ? (decimal)m.AE / m.AT * 100 : 0;
        if (aePercent >= 50 && m.AC <= 3)
            return MakeCard("FRICCIÓN", "chip-friction",
                "Se quedan explorando",
                $"El {aePercent:F0}% de las acciones son exploratorias. Ven tus redes pero no te contactan.",
                $"{m.AE} exploraciones · {m.AC} acciones de contacto", "Revisar CTAs", "/cards/mine", "explore-friction");

        // P4: Clicks without capture (ratio-based: capture rate < 15%)
        if (m.IC > 5 && (m.CC == 0 || (decimal)m.CC / m.IC < 0.15m))
            return MakeCard("FRICCIÓN", "chip-friction",
                "Hay clics, pero pocos contactos",
                $"Hay {m.IC} intenciones de contacto pero solo {m.CC} se confirman.",
                $"{m.IC} intenciones · {m.CC} contactos confirmados", "Revisar Embudo", "/cards/mine", "click-no-capture");

        // P5: No significant friction detected
        return MakeCard("FRICCIÓN", "chip-friction",
            "Sin fricción relevante",
            "No se detectaron cuellos de botella significativos en este período.",
            $"{m.IC} intenciones · {m.CC} contactos · {m.AE} exploraciones", "Ver Formulario", "/cards/mine", "no-friction");
    }

    private static SmartSummaryCard DetermineProximoPaso(
        string frictionScenario, SmartSummaryMetrics m)
    {
        return frictionScenario switch
        {
            "no-data" => MakeCard("PRÓXIMO PASO", "chip-action",
                "Comparte tu tarjeta",
                "Necesitas más tráfico. Comparte en redes, firma de email o en persona con QR.",
                "Objetivo: 80+ visitas", "Compartir", "/cards/mine", "action-share"),

            "form-abandon" => MakeCard("PRÓXIMO PASO", "chip-action",
                "Simplifica tu formulario",
                "Reduce los campos a nombre, email y mensaje. Menos campos = más envíos.",
                "3 campos convierten 2x más", "Editar Tarjeta", "/cards/mine", "action-form"),

            "explore-friction" => MakeCard("PRÓXIMO PASO", "chip-action",
                "Agrega un CTA de contacto",
                "Tus visitantes exploran pero no tienen un botón claro para contactarte.",
                "Un CTA visible duplica contactos", "Editar Tarjeta", "/cards/mine", "action-cta"),

            "click-no-capture" => MakeCard("PRÓXIMO PASO", "chip-action",
                "Revisa tu embudo",
                "La intención existe pero no se convierte. Verifica que WhatsApp, email y formulario funcionen.",
                $"Meta: convertir {Math.Max(3, m.IC / 3)} de {m.IC} intenciones",
                "Revisar Tarjeta", "/cards/mine", "action-funnel"),

            "no-friction" => MakeCard("PRÓXIMO PASO", "chip-action",
                "Mantén el ritmo",
                "Todo funciona bien. Sigue compartiendo tu tarjeta y considera agregar nuevos servicios.",
                m.PrevV > 0
                    ? $"Meta sugerida: +20% vs período anterior ({m.PrevV} → {(int)(m.PrevV * 1.2)})"
                    : "Objetivo: aumentar visitas esta semana",
                "Compartir Tarjeta", "/cards/mine", "action-maintain"),

            _ => MakeCard("PRÓXIMO PASO", "chip-action",
                "Optimiza tu tarjeta",
                "Revisa que todos los enlaces y CTAs estén actualizados y funcionando.",
                "Revisa enlaces cada 2 semanas", "Editar Tarjeta", "/cards/mine", "action-optimize")
        };
    }

    // ───── Smart Summary Helpers ─────

    private static SmartSummaryCard MakeCard(
        string chip, string chipColor, string title, string description,
        string baseLine, string? ctaText, string? ctaRoute, string scenarioKey)
    {
        return new SmartSummaryCard(chip, chipColor, title, description, baseLine,
            ctaText, ctaRoute, scenarioKey);
    }

    private static string FormatDelta(decimal delta)
    {
        var sign = delta >= 0 ? "+" : "";
        return $"{sign}{delta:F0}%";
    }

    private static SmartSummaryData EmptySmartSummary()
    {
        var emptyMetrics = new SmartSummaryMetrics(0, 0, 0, 0, 0, 0, 0, 0,
            DataQuality.Baja, 0, 0, 0, 0);
        var emptyCard = MakeCard("", "", "", "", "", null, null, "empty");
        return new SmartSummaryData(emptyMetrics, emptyCard, emptyCard, emptyCard, emptyCard,
            new List<ChannelBreakdown>(), new List<ChannelBreakdown>(), HasData: false);
    }

    #endregion
}
