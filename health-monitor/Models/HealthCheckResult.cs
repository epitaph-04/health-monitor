using health_monitor.Client.Model;

namespace health_monitor.Models;

public class HealthCheckResult
{
    public Status Status { get; set; } = Status.Unknown;
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastCheckedUtc { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = null!;
}