using System.ComponentModel.DataAnnotations;

namespace AIComplaintAgent.Models;

public class ComplaintInputModel
{
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ComplaintText { get; set; } = string.Empty;
}

public class ComplaintResultModel
{
    [Key]
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ComplaintText { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Sentiment { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string CustomerResponse { get; set; } = string.Empty;

    public string DecidedAction { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}