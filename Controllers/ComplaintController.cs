using AIComplaintAgent.Data;
using AIComplaintAgent.Models;
using AIComplaintAgent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIComplaintAgent.Controllers;

public class ComplaintController : Controller
{
    private readonly AppDbContext _db;
    private readonly ComplaintService _complaintService;
    private readonly SpeechService _speechService;
    private readonly IConfiguration _config;



    public ComplaintController(
    AppDbContext db,
    ComplaintService complaintService,
    SpeechService speechService,
    IConfiguration config)
    {
        _db = db;
        _complaintService = complaintService;
        _speechService = speechService;
        _config = config;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Analyze(ComplaintInputModel input)
    {
        // Check content safety before processing
        var (isHarmful, reason) = await _complaintService
            .CheckContentSafetyAsync(input.ComplaintText);

        if (isHarmful)
        {
            ViewBag.ErrorMessage = $"Your complaint contains inappropriate content ({reason}). Please rewrite it respectfully.";
            return View("Index");
        }

        // Agent analyzes and decides
        var complaint = await _complaintService
            .ProcessComplaintAsync(input);

        return View("Confirmation", complaint);
    }
    [HttpPost]
    public async Task<IActionResult> StartRecording()
    {
        var text = await _speechService.RecognizeSpeechAsync();

        if (text != null)
            return Json(new { success = true, text });

        return Json(new { success = false, text = "" });
    }
    [HttpGet]
    public async Task<IActionResult> GetSpeechToken()
    {
        var key = _config["AzureSpeech:Key"];
        var region = _config["AzureSpeech:Region"];

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

        var response = await client.PostAsync(
            $"https://{region}.api.cognitive.microsoft.com/sts/v1.0/issueToken",
            null);

        var token = await response.Content.ReadAsStringAsync();

        return Json(new { token, region });
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

    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var complaint = await _db.Complaints.FindAsync(id);
        if (complaint == null) return NotFound();

        complaint.Status = status;
        if (status == "Resolved")
            complaint.ResolvedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return RedirectToAction("Dashboard");
    }
}