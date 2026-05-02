using AIComplaintAgent.Agents;
using AIComplaintAgent.Data;
using AIComplaintAgent.Models;

namespace AIComplaintAgent.Services;

public class ComplaintService
{
    private readonly AppDbContext _db;
    private readonly ComplaintAgentService _agentService;
    private readonly ContentSafetyService _contentSafety;

    public ComplaintService(
        AppDbContext db,
        ComplaintAgentService agentService,
        ContentSafetyService contentSafety)
    {
        _db = db;
        _agentService = agentService;
        _contentSafety = contentSafety;
    }

    public async Task<(bool IsHarmful, string Reason)> CheckContentSafetyAsync(string text)
    {
        var result = await _contentSafety.AnalyzeAsync(text);
        return (result.IsHarmful, result.Reason);
    }

    public async Task<ComplaintResultModel> ProcessComplaintAsync(ComplaintInputModel input)
    {
        var agentResult = await _agentService.ProcessComplaintAsync(
            input.CustomerName,
            input.Email,
            input.ComplaintText);

        var json = agentResult.RawResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        var analysis = System.Text.Json.JsonSerializer
            .Deserialize<Dictionary<string, string>>(json);

        var complaint = new ComplaintResultModel
        {
            CustomerName = input.CustomerName,
            Email = input.Email,
            ComplaintText = input.ComplaintText,
            Type = analysis?["Type"] ?? "",
            Sentiment = analysis?["Sentiment"] ?? "",
            Priority = analysis?["Priority"] ?? "",
            Summary = analysis?["Summary"] ?? "",
            DecidedAction = analysis?["ActionTaken"] ?? "",
            CustomerResponse = analysis?["CustomerResponse"] ?? "",
            CreatedAt = DateTime.UtcNow,
            Status = "Open"
        };
        // Auto-resolve Low priority complaints
        if (complaint.Priority == "Low")
        {
            complaint.Status = "Resolved";
            complaint.ResolvedAt = DateTime.UtcNow;
           
        }
        _db.Complaints.Add(complaint);
        await _db.SaveChangesAsync();

        return complaint;
    }
}