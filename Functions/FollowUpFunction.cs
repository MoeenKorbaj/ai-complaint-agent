using AIComplaintAgent.Agents;
using AIComplaintAgent.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIComplaintAgent.Functions;

public class FollowUpFunction
{
    private readonly AppDbContext _db;
    private readonly FollowUpAgent _followUpAgent;
    private readonly ILogger<FollowUpFunction> _logger;

    public FollowUpFunction(
        AppDbContext db,
        FollowUpAgent followUpAgent,
        ILogger<FollowUpFunction> logger)
    {
        _db = db;
        _followUpAgent = followUpAgent;
        _logger = logger;
    }

    [Function("FollowUpFunction")]
    public async Task Run(
        [TimerTrigger("0 0 */6 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("FollowUpAgent started at: {time}", DateTime.UtcNow);

        var now = DateTime.UtcNow;

        // Fetch pending(not finished complaints
        var pendingComplaints = await _db.Complaints
            .Where(c => c.Status != "Resolved" &&
                   ((c.Priority == "High" && c.CreatedAt < now.AddHours(-24)) ||
                    (c.Priority == "Medium" && c.CreatedAt < now.AddHours(-48))))
            .ToListAsync();

        _logger.LogInformation("Found {count} pending complaints", pendingComplaints.Count);

        foreach (var complaint in pendingComplaints)
        {
            var hoursElapsed = (int)(now - complaint.CreatedAt).TotalHours;

            var result = await _followUpAgent.ProcessAsync(
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

        _logger.LogInformation("FollowUpAgent completed at: {time}", DateTime.UtcNow);
    }
}