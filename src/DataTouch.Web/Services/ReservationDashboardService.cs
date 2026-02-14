using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Dashboard analytics for reservation requests.
/// Uses IDbContextFactory (same pattern as AppointmentDashboardService).
/// </summary>
public class ReservationDashboardService
{
    private readonly IDbContextFactory<DataTouchDbContext> _factory;

    public ReservationDashboardService(IDbContextFactory<DataTouchDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// KPIs for the reservations dashboard.
    /// </summary>
    public async Task<ReservationKpis> GetKpisAsync(Guid organizationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var all = await db.ReservationRequests
            .Where(r => r.OrganizationId == organizationId)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var last30 = now.AddDays(-30);

        return new ReservationKpis
        {
            Total = all.Count,
            New = all.Count(r => r.Status == ReservationStatus.New),
            Confirmed = all.Count(r => r.Status == ReservationStatus.Confirmed),
            Cancelled = all.Count(r => r.Status == ReservationStatus.Cancelled),
            Last30Days = all.Count(r => r.CreatedAt >= last30),
            AvgNights = all.Any() ? Math.Round(all.Average(r => r.Nights), 1) : 0
        };
    }

    /// <summary>
    /// Status distribution for donut chart.
    /// </summary>
    public async Task<List<ChartItem>> GetStatusDistributionAsync(Guid organizationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.ReservationRequests
            .Where(r => r.OrganizationId == organizationId)
            .GroupBy(r => r.Status)
            .Select(g => new ChartItem { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync();
    }

    /// <summary>
    /// Reservations by day of week for bar chart.
    /// </summary>
    public async Task<List<ChartItem>> GetByDayOfWeekAsync(Guid organizationId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var items = await db.ReservationRequests
            .Where(r => r.OrganizationId == organizationId)
            .ToListAsync();

        var dayNames = new[] { "Dom", "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb" };
        return Enumerable.Range(0, 7)
            .Select(d => new ChartItem
            {
                Label = dayNames[d],
                Value = items.Count(r => (int)r.CreatedAt.DayOfWeek == d)
            })
            .ToList();
    }

    /// <summary>
    /// Reservations by period (last N days) for bar chart.
    /// </summary>
    public async Task<List<ChartItem>> GetByPeriodAsync(Guid organizationId, int days = 30)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var since = DateTime.UtcNow.AddDays(-days);
        var items = await db.ReservationRequests
            .Where(r => r.OrganizationId == organizationId && r.CreatedAt >= since)
            .ToListAsync();

        return Enumerable.Range(0, days)
            .Select(i =>
            {
                var date = since.AddDays(i).Date;
                return new ChartItem
                {
                    Label = date.ToString("dd/MM"),
                    Value = items.Count(r => r.CreatedAt.Date == date)
                };
            })
            .ToList();
    }

    public class ReservationKpis
    {
        public int Total { get; set; }
        public int New { get; set; }
        public int Confirmed { get; set; }
        public int Cancelled { get; set; }
        public int Last30Days { get; set; }
        public double AvgNights { get; set; }
    }

    public class ChartItem
    {
        public string Label { get; set; } = "";
        public int Value { get; set; }
    }
}
