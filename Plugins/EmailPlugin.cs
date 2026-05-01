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

    [KernelFunction("send_team_alert")]
    [Description("Send urgent alert to support team when complaint is High Priority. Use this to notify the team.")]
    public async Task<string> SendTeamAlert(
        [Description("Customer name")] string customerName,
        [Description("Customer email")] string customerEmail,
        [Description("Complaint summary")] string summary,
        [Description("Complaint priority")] string priority)
    {
        try
        {
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            var teamEmail = _config["Email:Username"]; // same Gmail

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AI Complaint Agent", username));
            message.To.Add(new MailboxAddress("Support Team", teamEmail));
            message.Subject = $"🚨 Urgent Complaint - {customerName}";
            message.Body = new TextPart("html")
            {
                Text = $"""
                    <h2>Urgent Customer Complaint</h2>
                    <p><strong>Customer:</strong> {customerName}</p>
                    <p><strong>Email:</strong> {customerEmail}</p>
                    <p><strong>Summary:</strong> {summary}</p>
                    <p><strong>Priority:</strong> {priority}</p>
                    <p>Please respond immediately.</p>
                    """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return $"✅ Team alert sent for {customerName}";
        }
        catch (Exception ex)
        {
            return $"❌ Team alert failed: {ex.Message}";
        }
    }

    [KernelFunction("send_customer_followup")]
    [Description("Send follow-up email to the customer. Use this to apologize or update the customer about their complaint status.")]
    public async Task<string> SendCustomerFollowup(
        [Description("Customer name")] string customerName,
        [Description("Customer email address")] string customerEmail,
        [Description("Follow-up message in customer's language")] string followupMessage)
    {
        try
        {
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("AI Complaint Agent", username));
            message.To.Add(new MailboxAddress(customerName, customerEmail));
            message.Subject = $"Update on Your Complaint - {customerName}";
            message.Body = new TextPart("html")
            {
                Text = $"""
                    <p>{followupMessage}</p>
                    <br/>
                    <p>AI Complaint Agent</p>
                    """
            };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return $"✅ Follow-up email sent to {customerEmail}";
        }
        catch (Exception ex)
        {
            return $"❌ Follow-up email failed: {ex.Message}";
        }
    }
}