using System.Text;
using System.Text.Json;
using health_monitor.Models;

namespace health_monitor.Services.Alerting;

public class WebhookNotificationChannel : INotificationChannel
{
    private readonly WebhookConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookNotificationChannel> _logger;

    public WebhookNotificationChannel(WebhookConfiguration config, HttpClient httpClient, ILogger<WebhookNotificationChannel> logger)
    {
        _config = config;
        _httpClient = httpClient;
        _logger = logger;
    }

    public string ChannelId => _config.ChannelId;
    public string ChannelType => "Webhook";

    public async Task<bool> SendNotification(AlertLevel level, string serviceId, string message, Dictionary<string, object>? metadata = null)
    {
        try
        {
            var payload = new
            {
                alertLevel = level.ToString(),
                serviceId,
                message,
                timestamp = DateTime.UtcNow,
                metadata = metadata ?? new Dictionary<string, object>()
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add custom headers
            var request = new HttpRequestMessage(HttpMethod.Post, _config.Url)
            {
                Content = content
            };

            foreach (var header in _config.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add signature if secret is provided
            if (!string.IsNullOrEmpty(_config.Secret))
            {
                var signature = GenerateSignature(json, _config.Secret);
                request.Headers.TryAddWithoutValidation("X-Health-Monitor-Signature", signature);
            }

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook alert sent for service {ServiceId} to {Url}", serviceId, _config.Url);
                return true;
            }
            else
            {
                _logger.LogWarning("Webhook alert failed for service {ServiceId}. Status: {StatusCode}", serviceId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook alert for service {ServiceId} to {Url}", serviceId, _config.Url);
            return false;
        }
    }

    public async Task<bool> TestConnection()
    {
        try
        {
            var testPayload = new
            {
                test = true,
                timestamp = DateTime.UtcNow,
                message = "Health Monitor webhook test"
            };

            var json = JsonSerializer.Serialize(testPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _config.Url)
            {
                Content = content
            };

            foreach (var header in _config.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateSignature(string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLower();
    }
}

public class WebhookConfiguration
{
    public string ChannelId { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Secret { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}