using health_monitor.Models;

namespace health_monitor.Services.Alerting;

public interface INotificationChannel
{
    string ChannelId { get; }
    string ChannelType { get; }
    Task<bool> SendNotification(AlertLevel level, string serviceId, string message, Dictionary<string, object>? metadata = null);
    Task<bool> TestConnection();
}