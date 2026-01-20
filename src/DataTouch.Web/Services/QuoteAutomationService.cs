using DataTouch.Domain.Entities;
using DataTouch.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DataTouch.Web.Services;

/// <summary>
/// Background service for Quote Request automations (SLA alerts, reminders).
/// Runs every 15 minutes to check for overdue quotes and send notifications.
/// </summary>
public class QuoteAutomationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QuoteAutomationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(15);

    public QuoteAutomationService(
        IServiceProvider serviceProvider,
        ILogger<QuoteAutomationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QuoteAutomationService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSlaAlertsAsync(stoppingToken);
                await ProcessReminderNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuoteAutomationService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    /// <summary>
    /// Check for quotes that have exceeded their SLA deadline and mark them.
    /// </summary>
    private async Task ProcessSlaAlertsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataTouchDbContext>();

        var now = DateTime.UtcNow;

        // Find quotes that are overdue and haven't been notified
        var overdueQuotes = await db.QuoteRequests
            .Where(q => q.Status == QuoteStatus.New || q.Status == QuoteStatus.InReview)
            .Where(q => q.SlaDeadlineAt != null && q.SlaDeadlineAt < now)
            .Where(q => !q.SlaNotified)
            .ToListAsync(stoppingToken);

        foreach (var quote in overdueQuotes)
        {
            _logger.LogWarning(
                "SLA exceeded for quote {RequestNumber} (exceeded by {Hours:F1}h)",
                quote.RequestNumber,
                (now - quote.SlaDeadlineAt!.Value).TotalHours);

            // Mark as notified
            quote.SlaNotified = true;

            // Log activity
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                OrganizationId = quote.OrganizationId,
                EntityType = "QuoteRequest",
                EntityId = quote.Id,
                Type = ActivityType.SlaAlert,
                Description = $"⚠️ SLA vencido: sin respuesta en tiempo",
                SystemSource = "automation",
                CreatedAt = DateTime.UtcNow
            };
            db.Activities.Add(activity);

            // TODO: Send email/push notification to admin
        }

        if (overdueQuotes.Any())
        {
            await db.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Processed {Count} SLA alerts", overdueQuotes.Count);
        }
    }

    /// <summary>
    /// Send reminder notifications for quotes approaching SLA deadline.
    /// </summary>
    private async Task ProcessReminderNotificationsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataTouchDbContext>();

        var now = DateTime.UtcNow;
        var warningThreshold = TimeSpan.FromHours(2); // Warn 2 hours before deadline

        // Find quotes approaching SLA deadline
        var approachingQuotes = await db.QuoteRequests
            .Where(q => q.Status == QuoteStatus.New || q.Status == QuoteStatus.InReview)
            .Where(q => q.SlaDeadlineAt != null)
            .Where(q => q.SlaDeadlineAt > now && q.SlaDeadlineAt < now.Add(warningThreshold))
            .Where(q => !q.SlaNotified) // Haven't been warned yet
            .ToListAsync(stoppingToken);

        foreach (var quote in approachingQuotes)
        {
            var remaining = quote.SlaDeadlineAt!.Value - now;
            
            _logger.LogInformation(
                "SLA warning for quote {RequestNumber} ({Minutes} min remaining)",
                quote.RequestNumber,
                (int)remaining.TotalMinutes);

            // Log activity
            var activity = new Activity
            {
                Id = Guid.NewGuid(),
                OrganizationId = quote.OrganizationId,
                EntityType = "QuoteRequest",
                EntityId = quote.Id,
                Type = ActivityType.SlaAlert,
                Description = $"⏰ Recordatorio: {(int)remaining.TotalMinutes} min para vencer SLA",
                SystemSource = "automation",
                CreatedAt = DateTime.UtcNow
            };
            db.Activities.Add(activity);

            // TODO: Send push notification
        }

        if (approachingQuotes.Any())
        {
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}

/// <summary>
/// Extension methods to register QuoteAutomationService.
/// </summary>
public static class QuoteAutomationServiceExtensions
{
    public static IServiceCollection AddQuoteAutomations(this IServiceCollection services)
    {
        services.AddHostedService<QuoteAutomationService>();
        return services;
    }
}
