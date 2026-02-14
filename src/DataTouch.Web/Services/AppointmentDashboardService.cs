using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// BI queries for the Appointments dashboard.
/// Uses IDbContextFactory per EF Core Concurrency Guardrails (CLAUDE.md).
/// Provides KPIs, status distribution, daily volume, and hourly heatmap data.
/// </summary>
public class AppointmentDashboardService
{
    private readonly IDbContextFactory<DataTouchDbContext> _contextFactory;

    public AppointmentDashboardService(IDbContextFactory<DataTouchDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    #region Data Models

    public record AppointmentKpis(
        int TotalAppointments,
        int Pending,
        int Confirmed,
        int Completed,
        int Cancelled,
        int NoShow,
        decimal CompletionRate,
        decimal CancellationRate,
        decimal NoShowRate
    );

    public record StatusDistribution(string Status, int Count, string Color);
    public record DailyVolume(string Date, int Booked, int Completed, int Cancelled);
    public record HourlyHeatmap(int Hour, int Monday, int Tuesday, int Wednesday, int Thursday, int Friday, int Saturday, int Sunday);

    #endregion

    /// <summary>
    /// Get KPI summary for a card's appointments within a date range.
    /// </summary>
    public async Task<AppointmentKpis> GetKpisAsync(Guid cardId, DateTime from, DateTime to)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var appointments = await db.Appointments
            .Where(a => a.CardId == cardId && a.CreatedAt >= from && a.CreatedAt <= to)
            .ToListAsync();

        var total = appointments.Count;
        var pending = appointments.Count(a => a.Status == AppointmentStatus.Pending);
        var confirmed = appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
        var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        var noShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

        var completionRate = total > 0 ? Math.Round((decimal)completed / total * 100, 1) : 0;
        var cancellationRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 1) : 0;
        var noShowRate = total > 0 ? Math.Round((decimal)noShow / total * 100, 1) : 0;

        return new AppointmentKpis(total, pending, confirmed, completed, cancelled, noShow, completionRate, cancellationRate, noShowRate);
    }

    /// <summary>
    /// Get status distribution for pie/donut chart.
    /// </summary>
    public async Task<List<StatusDistribution>> GetStatusDistributionAsync(Guid cardId, DateTime from, DateTime to)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var appointments = await db.Appointments
            .Where(a => a.CardId == cardId && a.CreatedAt >= from && a.CreatedAt <= to)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return appointments.Select(a => new StatusDistribution(
            a.Status.ToString(),
            a.Count,
            a.Status switch
            {
                AppointmentStatus.Pending => "#F59E0B",
                AppointmentStatus.Confirmed => "#3B82F6",
                AppointmentStatus.Completed => "#22C55E",
                AppointmentStatus.Cancelled => "#EF4444",
                AppointmentStatus.NoShow => "#8B5CF6",
                _ => "#6B7280"
            }
        )).OrderByDescending(s => s.Count).ToList();
    }

    /// <summary>
    /// Get daily appointment volume for the bar/line chart.
    /// </summary>
    public async Task<List<DailyVolume>> GetDailyVolumeAsync(Guid cardId, DateTime from, DateTime to)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var appointments = await db.Appointments
            .Where(a => a.CardId == cardId && a.CreatedAt >= from && a.CreatedAt <= to)
            .ToListAsync();

        var days = Enumerable.Range(0, (int)(to - from).TotalDays + 1)
            .Select(d => from.AddDays(d).Date)
            .ToList();

        return days.Select(day =>
        {
            var dayAppts = appointments.Where(a => a.CreatedAt.Date == day).ToList();
            return new DailyVolume(
                day.ToString("yyyy-MM-dd"),
                dayAppts.Count,
                dayAppts.Count(a => a.Status == AppointmentStatus.Completed),
                dayAppts.Count(a => a.Status == AppointmentStatus.Cancelled)
            );
        }).ToList();
    }

    /// <summary>
    /// Get hourly heatmap data (hour x day-of-week) for appointment density.
    /// </summary>
    public async Task<List<HourlyHeatmap>> GetHourlyHeatmapAsync(Guid cardId, DateTime from, DateTime to)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var appointments = await db.Appointments
            .Where(a => a.CardId == cardId && a.CreatedAt >= from && a.CreatedAt <= to
                && a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        var heatmap = new List<HourlyHeatmap>();
        for (int h = 0; h < 24; h++)
        {
            var hourAppts = appointments.Where(a => a.StartDateTime.Hour == h).ToList();
            heatmap.Add(new HourlyHeatmap(
                h,
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Monday),
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Tuesday),
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Wednesday),
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Thursday),
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Friday),
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Saturday),
                hourAppts.Count(a => a.StartDateTime.DayOfWeek == DayOfWeek.Sunday)
            ));
        }
        return heatmap;
    }
}
