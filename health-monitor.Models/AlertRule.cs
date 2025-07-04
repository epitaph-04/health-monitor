namespace health_monitor.Models;

public class AlertRule
{
    public string Id { get; set; } = null!;
    public string ServiceId { get; set; } = null!;
    public AlertLevel Level { get; set; }
    public int ConsecutiveFailures { get; set; } = 1;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(5);
    public string[] NotificationChannels { get; set; } = [];
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, string> Metadata { get; set; } = new();
}