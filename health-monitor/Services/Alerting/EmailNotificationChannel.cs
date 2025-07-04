using System.Net.Mail;
using System.Net;
using health_monitor.Models;

namespace health_monitor.Services.Alerting;

public class EmailNotificationChannel : INotificationChannel
{
    private readonly EmailConfiguration _config;
    private readonly ILogger<EmailNotificationChannel> _logger;

    public EmailNotificationChannel(EmailConfiguration config, ILogger<EmailNotificationChannel> logger)
    {
        _config = config;
        _logger = logger;
    }

    public string ChannelId => _config.ChannelId;
    public string ChannelType => "Email";

    public async Task<bool> SendNotification(AlertLevel level, string serviceId, string message, Dictionary<string, object>? metadata = null)
    {
        try
        {
            using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
            {
                EnableSsl = _config.EnableSsl,
                Credentials = new NetworkCredential(_config.Username, _config.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config.FromAddress, _config.FromName),
                Subject = $"[{level}] Health Check Alert - {serviceId}",
                Body = GenerateEmailBody(level, serviceId, message, metadata),
                IsBodyHtml = true
            };

            foreach (var recipient in _config.Recipients)
            {
                mailMessage.To.Add(recipient);
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email alert sent for service {ServiceId} to {Recipients}", serviceId, string.Join(", ", _config.Recipients));
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email alert for service {ServiceId}", serviceId);
            return false;
        }
    }

    public async Task<bool> TestConnection()
    {
        try
        {
            using var client = new SmtpClient(_config.SmtpHost, _config.SmtpPort)
            {
                EnableSsl = _config.EnableSsl,
                Credentials = new NetworkCredential(_config.Username, _config.Password)
            };
            
            // Just connect and disconnect to test
            await client.SendMailAsync(new MailMessage(_config.FromAddress, _config.Recipients.First(), "Test", "Test connection"));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateEmailBody(AlertLevel level, string serviceId, string message, Dictionary<string, object>? metadata)
    {
        var html = $@"
            <html>
            <head>
                <style>
                    .alert-header {{ background-color: {GetColorForLevel(level)}; color: white; padding: 10px; font-weight: bold; }}
                    .alert-content {{ padding: 20px; font-family: Arial, sans-serif; }}
                    .metadata {{ background-color: #f5f5f5; padding: 10px; margin-top: 10px; }}
                </style>
            </head>
            <body>
                <div class='alert-header'>
                    Health Check Alert - {level}
                </div>
                <div class='alert-content'>
                    <h3>Service: {serviceId}</h3>
                    <p><strong>Message:</strong> {message}</p>
                    <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>";

        if (metadata != null && metadata.Any())
        {
            html += "<div class='metadata'><h4>Additional Information:</h4><ul>";
            foreach (var kvp in metadata)
            {
                html += $"<li><strong>{kvp.Key}:</strong> {kvp.Value}</li>";
            }
            html += "</ul></div>";
        }

        html += @"
                </div>
            </body>
            </html>";

        return html;
    }

    private string GetColorForLevel(AlertLevel level)
    {
        return level switch
        {
            AlertLevel.Info => "#2196F3",
            AlertLevel.Warning => "#FF9800",
            AlertLevel.Critical => "#F44336",
            AlertLevel.Emergency => "#9C27B0",
            _ => "#666666"
        };
    }
}

public class EmailConfiguration
{
    public string ChannelId { get; set; } = null!;
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string FromName { get; set; } = "Health Monitor";
    public string[] Recipients { get; set; } = [];
}