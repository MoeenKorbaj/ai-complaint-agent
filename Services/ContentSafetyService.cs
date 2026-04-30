using Azure;
using Azure.AI.ContentSafety;

namespace AIComplaintAgent.Services;

public class ContentSafetyService
{
    private readonly ContentSafetyClient _client;

    public ContentSafetyService(IConfiguration config)
    {
        var endpoint = config["ContentSafety:Endpoint"];
        var apiKey = config["ContentSafety:ApiKey"];
        _client = new ContentSafetyClient(
            new Uri(endpoint!),
            new AzureKeyCredential(apiKey!));
    }

    public async Task<ContentSafetyResult> AnalyzeAsync(string text)
    {
        var request = new AnalyzeTextOptions(text);
        var response = await _client.AnalyzeTextAsync(request);

        var result = new ContentSafetyResult();

        foreach (var category in response.Value.CategoriesAnalysis)
        {
            if (category.Severity >= 2)
            {
                result.IsHarmful = true;
                result.Reason = category.Category.ToString();
                break;
            }
        }

        return result;
    }
}

public class ContentSafetyResult
{
    public bool IsHarmful { get; set; } = false;
    public string Reason { get; set; } = string.Empty;
}