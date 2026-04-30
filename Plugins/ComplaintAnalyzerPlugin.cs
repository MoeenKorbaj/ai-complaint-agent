using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIComplaintAgent.Plugins;

public class ComplaintAnalyzerPlugin
{
    [KernelFunction]
    [Description("Analyze a customer complaint and extract type, sentiment, priority, and summary")]
    public string AnalyzeComplaint(
        [Description("The customer complaint text")] string complaint)
    {
        return $"Analyzing: {complaint}";
    }

    [KernelFunction]
    [Description("Decide the appropriate action based on complaint analysis")]
    public string DecideAction(
        [Description("The priority of the complaint")] string priority,
        [Description("The type of the complaint")] string type)
    {
        return priority switch
        {
            "High" => "SendUrgentEmail",
            "Medium" => "CreateTicket",
            _ => "LogAndMonitor"
        };
    }
}