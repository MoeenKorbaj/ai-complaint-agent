#pragma warning disable SKEXP01
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110

using AIComplaintAgent.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AIComplaintAgent.Agents;

public class FollowUpAgent
{
    private readonly Kernel _kernel;
    private readonly EmailPlugin _emailPlugin;

    public FollowUpAgent(Kernel kernel, EmailPlugin emailPlugin)
    {
        _kernel = kernel;
        _emailPlugin = emailPlugin;
        _kernel.Plugins.AddFromObject(_emailPlugin, "EmailPlugin");
    }

    public async Task<string> ProcessAsync(
        int complaintId,
        string customerName,
        string customerEmail,
        string type,
        string priority,
        string summary,
        string originalResponse,
        int hoursElapsed)
    {
        var agent = new ChatCompletionAgent
        {
            Name = "FollowUpAgent",
            Instructions = """
    You are a customer complaint follow-up specialist.
    Your job is to review pending complaints and decide if action is needed.
    
    Available tools:
    - send_team_alert: Alert support team to take action
    - send_customer_followup: Send apology/update email to customer
    
    Decision guidelines:
    
    HIGH Priority:
    - 24-48 hours → send_customer_followup (apology) + send_team_alert
    - 48-72 hours → send_customer_followup (urgent apology) + send_team_alert
    - 72+ hours   → send_customer_followup (immediate action promise) + send_team_alert
    
    MEDIUM Priority:
    - 24-72 hours → send_customer_followup only
    - 72+ hours   → send_customer_followup + send_team_alert
    
    LOW Priority:
    - No action needed
    
    IMPORTANT:
    - Write customer email in SAME language as Original Response
    - Be empathetic and professional
    - Mention complaint reference number in email
    """,
            Kernel = _kernel,
            Arguments = new KernelArguments(
                new AzureOpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                })
        };

        var message = $"""
            Complaint ID: {complaintId}
            Customer: {customerName}
            Email: {customerEmail}
            Type: {type}
            Priority: {priority}
            Summary: {summary}
            Original Response: {originalResponse}
            Hours Elapsed: {hoursElapsed} hours
            
            Decide if this complaint needs a follow-up action.
            """;

        var chat = new AgentGroupChat(agent);
        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, message));

        var result = string.Empty;
        await foreach (var response in chat.InvokeAsync())
        {
            result = response.Content ?? "";
        }

        return result;
    }
}