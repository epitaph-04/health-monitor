using health_monitor.Models;
using health_monitor.Client.Model;
using System.Collections.Concurrent;

namespace health_monitor.Services.Alerting;

public class AlertingService : IAlertingService
{
    private readonly ILogger<AlertingService> _logger;
    private readonly ConcurrentDictionary<string, INotificationChannel> _channels = new();
    private readonly ConcurrentDictionary<string, AlertRule> _alertRules = new();
    private readonly ConcurrentDictionary<string, AlertState> _alertStates = new();

    public AlertingService(ILogger<AlertingService> logger)
    {
        _logger = logger;
    }

    public async Task SendAlert(AlertLevel level, string serviceId, string message, Dictionary<string, object>? metadata = null)
    {
        var alertRules = _alertRules.Values.Where(r => r.ServiceId == serviceId && r.IsEnabled).ToArray();
        
        foreach (var rule in alertRules)
        {
            foreach (var channelId in rule.NotificationChannels)
            {
                if (_channels.TryGetValue(channelId, out var channel))
                {
                    try
                    {
                        await channel.SendNotification(level, serviceId, message, metadata);
                        _logger.LogInformation("Alert sent for service {ServiceId} via {ChannelType}", serviceId, channel.ChannelType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send alert for service {ServiceId} via {ChannelType}", serviceId, channel.ChannelType);
                    }
                }
            }
        }
    }

    public Task ConfigureAlertRules(AlertRule[] rules)
    {
        _alertRules.Clear();
        foreach (var rule in rules)
        {
            _alertRules[rule.Id] = rule;
        }
        _logger.LogInformation("Configured {Count} alert rules", rules.Length);
        return Task.CompletedTask;
    }

    public Task<AlertRule[]> GetAlertRules()
    {
        return Task.FromResult(_alertRules.Values.ToArray());
    }

    public Task<bool> ShouldTriggerAlert(string serviceId, HealthCheckResult result)
    {
        var alertRules = _alertRules.Values.Where(r => r.ServiceId == serviceId && r.IsEnabled).ToArray();
        
        foreach (var rule in alertRules)
        {
            var alertState = _alertStates.GetOrAdd(rule.Id, _ => new AlertState());
            
            if (result.Status == Status.Critical)
            {
                alertState.ConsecutiveFailures++;
                alertState.LastFailureTime = DateTime.UtcNow;
                
                if (alertState.ConsecutiveFailures >= rule.ConsecutiveFailures)
                {
                    var timeSinceFirstFailure = DateTime.UtcNow - alertState.FirstFailureTime;
                    if (timeSinceFirstFailure >= rule.Duration)
                    {
                        return Task.FromResult(true);
                    }
                }
            }
            else
            {
                alertState.Reset();
            }
        }
        
        return Task.FromResult(false);
    }

    public Task RegisterNotificationChannel(string channelId, INotificationChannel channel)
    {
        _channels[channelId] = channel;
        _logger.LogInformation("Registered notification channel {ChannelId} of type {ChannelType}", channelId, channel.ChannelType);
        return Task.CompletedTask;
    }

    private class AlertState
    {
        public int ConsecutiveFailures { get; set; }
        public DateTime FirstFailureTime { get; set; } = DateTime.UtcNow;
        public DateTime LastFailureTime { get; set; } = DateTime.UtcNow;
        
        public void Reset()
        {
            ConsecutiveFailures = 0;
            FirstFailureTime = DateTime.UtcNow;
        }
    }
}