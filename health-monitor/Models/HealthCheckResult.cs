namespace health_monitor.Models;

public class HealthCheckResult(string applicationName)
{
    public string ApplicationName { get; set; } = applicationName;
    public HealthStatus Status { get; set; } = HealthStatus.Unknown;
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastCheckedUtc { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
}