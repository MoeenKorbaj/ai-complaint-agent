using AIComplaintAgent.Data;
using AIComplaintAgent.Models;
using AIComplaintAgent.Plugins;
using AIComplaintAgent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;


namespace AIComplaintAgent.Controllers;

public class ComplaintController : Controller
{
    private readonly Kernel _kernel;
    private readonly AppDbContext _db;
    private readonly EmailPlugin _emailPlugin;
    private readonly ContentSafetyService _contentSafety;

    public ComplaintController(
        Kernel kernel,
        AppDbContext db,
        EmailPlugin emailPlugin,
        ContentSafetyService contentSafety)
    {
        _kernel = kernel;
        _db = db;
        _emailPlugin = emailPlugin;
        _contentSafety = contentSafety;
        _kernel.Plugins.AddFromType<ComplaintAnalyzerPlugin>();
        _kernel.Plugins.AddFromObject(_emailPlugin);
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(ComplaintInputModel input)
    {
        var settings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        // فلترة المحتوى المسيء
        var safetyResult = await _contentSafety.AnalyzeAsync(input.ComplaintText);
        if (safetyResult.IsHarmful)
        {
            ViewBag.ErrorMessage = $"Your complaint contains inappropriate content ({safetyResult.Reason}). Please rewrite it respectfully.";
            return View("Index");
        }

        /* var prompt = $$"""
 You are a customer complaint analysis agent.
 Analyze this complaint and respond ONLY in this JSON format:
 {
     "Type": "Delivery/Payment/Refund/Technical/Other",
     "Sentiment": "Positive/Negative/Neutral",
     "Priority": "High/Medium/Low",
     "Summary": "brief summary",
     "DecidedAction": "SendUrgentEmail/CreateTicket/LogAndMonitor"
 }

 Customer: {{input.CustomerName}}
 Complaint: {{input.ComplaintText}}
 """;*/
        var prompt = $$"""
You are a customer complaint analysis agent.
Analyze this complaint and respond ONLY in this JSON format:
{
    "Type": "Delivery/Payment/Refund/Technical/Other",
    "Sentiment": "Positive/Negative/Neutral",
    "Priority": "High/Medium/Low",
    "Summary": "brief summary in English",
    "DecidedAction": "SendUrgentEmail/CreateTicket/LogAndMonitor",
    "CustomerResponse": "write this in the EXACT same language as the complaint text below"
}

CRITICAL RULES:
- CustomerResponse MUST be in the same language as the complaint
- If complaint is English → CustomerResponse in English
- If complaint is Arabic → CustomerResponse in Arabic
- If complaint is French → CustomerResponse in French
- Be empathetic and professional
- Mention response time: High = 2 hours, Medium = 24 hours, Low = 48 hours
- Never change the language under any circumstances

Customer: {{input.CustomerName}}
Complaint: {{input.ComplaintText}}
""";
        var result = await _kernel.InvokePromptAsync(prompt,
            new KernelArguments(settings));

        var json = result.ToString()
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
            DecidedAction = analysis?["DecidedAction"] ?? "",
            CustomerResponse = analysis?["CustomerResponse"] ?? "",
            CreatedAt = DateTime.UtcNow
        };

        // حفظ في قاعدة البيانات
        _db.Complaints.Add(complaint);
        await _db.SaveChangesAsync();
        // إرسال إيميل تلقائي إذا كانت الأولوية عالية
        if (complaint.Priority == "High")
        {
            var emailResult = await _emailPlugin.SendUrgentEmail(
        complaint.CustomerName,
        complaint.Email,
        complaint.Summary);

            Console.WriteLine($"Email Result: {emailResult}");
        }


        return View("Confirmation", complaint);
    }
    public async Task<IActionResult> Dashboard()
    {
        var complaints = await _db.Complaints
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(complaints);
    }
    public async Task<IActionResult> Details(int id)
    {
        var complaint = await _db.Complaints.FindAsync(id);
        if (complaint == null) return NotFound();
        return View("Result", complaint);
    }
}