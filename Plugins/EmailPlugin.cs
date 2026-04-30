using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.SemanticKernel;
using MimeKit;
using System.ComponentModel;

namespace AIComplaintAgent.Plugins;

public class EmailPlugin
{
    private readonly IConfiguration _config;

    public EmailPlugin(IConfiguration config)
    {
        _config = config;
    }

    [KernelFunction]
    [Description("Send urgent email notification for high priority complaint")]
    public async Task<string> SendUrgentEmail(
        [Description("Customer name")] string customerName,
        [Description("Customer email")] string customerEmail,
        [Description("Complaint summary")] string summary)
    {
        try
        {
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            var receiver = _config["Email:ReceiverEmail"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AI Complaint Agent", username));
            message.To.Add(new MailboxAddress("Support Team", receiver));
            message.Subject = $"🚨 Urgent Complaint - {customerName}";

            message.Body = new TextPart("html")
            {
                Text = $"""
                    <h2>Urgent Customer Complaint</h2>
                    <p><strong>Customer:</strong> {customerName}</p>
                    <p><strong>Email:</strong> {customerEmail}</p>
                    <p><strong>Summary:</strong> {summary}</p>
                    <p><strong>Priority:</strong> HIGH</p>
                    <p>Please respond immediately.</p>
                    """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return $"✅ Urgent email sent for {customerName}";
        }
        catch (Exception ex)
        {
            return $"❌ Email failed: {ex.Message}";
        }
    }
}
