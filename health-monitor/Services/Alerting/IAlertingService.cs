using health_monitor.Models;

namespace health_monitor.Services.Alerting;

public interface IAlertingService
{
    Task SendAlert(AlertLevel level, string serviceId, string message, Dictionary<string, object>? metadata = null);
    Task ConfigureAlertRules(AlertRule[] rules);
    Task<AlertRule[]> GetAlertRules();
    Task<bool> ShouldTriggerAlert(string serviceId, HealthCheckResult result);
    Task RegisterNotificationChannel(string channelId, INotificationChannel channel);
}