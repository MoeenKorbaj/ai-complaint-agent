using AIComplaintAgent.Agents;
using AIComplaintAgent.Data;
using Microsoft.EntityFrameworkCore;

namespace AIComplaintAgent.Services;

public class FollowUpBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<FollowUpBackgroundService> _logger;

    public FollowUpBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<FollowUpBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunFollowUpAsync();
            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }

    private async Task RunFollowUpAsync()
    {
        _logger.LogInformation("FollowUpAgent started at: {time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var followUpAgent = scope.ServiceProvider.GetRequiredService<FollowUpAgent>();

        var now = DateTime.UtcNow;

        var pendingComplaints = await db.Complaints
            .Where(c => c.Status != "Resolved" &&
                   ((c.Priority == "High" && c.CreatedAt < now.AddHours(-24)) ||
                    (c.Priority == "Medium" && c.CreatedAt < now.AddHours(-48))))
            .ToListAsync();

        _logger.LogInformation("Found {count} pending complaints", pendingComplaints.Count);

        foreach (var complaint in pendingComplaints)
        {
            var hoursElapsed = (int)(now - complaint.CreatedAt).TotalHours;

            var result = await followUpAgent.ProcessAsync(
                complaint.Id,
                complaint.CustomerName,
                complaint.Email,
                complaint.Type,
                complaint.Priority,
                complaint.Summary,
                complaint.CustomerResponse,
                hoursElapsed);

            _logger.LogInformation(
                "FollowUp for complaint {id}: {result}",
                complaint.Id, result);
        }
    }
}