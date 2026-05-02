#pragma warning disable SKEXP01
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110

using AIComplaintAgent.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AIComplaintAgent.Agents;

public class ComplaintAgentService
{
    private readonly Kernel _kernel;
    private readonly EmailPlugin _emailPlugin;

    public ComplaintAgentService(Kernel kernel, EmailPlugin emailPlugin)
    {
        _kernel = kernel;
        _emailPlugin = emailPlugin;
        _kernel.Plugins.AddFromObject(_emailPlugin, "EmailPlugin");
    }

    public async Task<ComplaintAgentResult> ProcessComplaintAsync(
        string customerName,
        string customerEmail,
        string complaintText)
    {
        var agent = new ChatCompletionAgent
        {
            Name = "ComplaintAgent",
            Instructions = """
    You are an expert customer complaint handling agent.
    
    FIRST: Determine if the text is actually a complaint.
    If it is NOT a complaint (e.g. general questions, greetings, random text):
    Respond with this JSON:
    {
        "Type": "NotAComplaint",
        "Sentiment": "Neutral",
        "Priority": "Low",
        "Summary": "Not a complaint",
        "CustomerResponse": "polite message in same language explaining this system is for complaints only",
        "ActionTaken": "Rejected - not a complaint"
    }
    
    If it IS a complaint:
    1. Analyze the complaint
    2. Determine: Type, Sentiment, Priority, Summary
    3. Generate a professional empathetic response in the SAME language as the complaint
    4. If Priority is High → MUST call send_team_alert tool
    
    You decide everything based on your analysis.
    No fixed rules — use your judgment.
    
    Always respond with this JSON at the end:
    {
        "Type": "Delivery/Payment/Refund/Technical/Other",
        "Sentiment": "Positive/Negative/Neutral", 
        "Priority": "High/Medium/Low",
        "Summary": "brief summary in English",
        "CustomerResponse": "response in same language as complaint",
        "ActionTaken": "what you did"
    }
    """,
            Kernel = _kernel,
            Arguments = new KernelArguments(
        new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        })
        };

        var chat = new AgentGroupChat(agent);

        chat.AddChatMessage(
    new ChatMessageContent(
        AuthorRole.User,
        $"Customer: {customerName}\nEmail: {customerEmail}\nComplaint: {complaintText}"));

        var result = new ComplaintAgentResult();

        await foreach (var message in chat.InvokeAsync())
        {
            result.RawResponse = message.Content ?? "";
        }

        return result;
    }
}

public class ComplaintAgentResult
{
    public string RawResponse { get; set; } = string.Empty;
}