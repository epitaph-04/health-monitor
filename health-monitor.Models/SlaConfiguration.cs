namespace health_monitor.Models;

public class SlaConfiguration
{
    public string ServiceId { get; set; } = null!;
    public double TargetAvailability { get; set; } = 99.9; // 99.9%
    public TimeSpan MaxResponseTime { get; set; } = TimeSpan.FromSeconds(5);
    public SlaReportingPeriod Period { get; set; } = SlaReportingPeriod.Monthly;
    public bool IsEnabled { get; set; } = true;
    public string[] NotificationChannels { get; set; } = [];
}

public enum SlaReportingPeriod
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly
}

public class SlaReport
{
    public string ServiceId { get; set; } = null!;
    public SlaReportingPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double ActualAvailability { get; set; }
    public double TargetAvailability { get; set; }
    public bool SlaAchieved { get; set; }
    public TimeSpan TotalDowntime { get; set; }
    public TimeSpan AllowedDowntime { get; set; }
    public double AverageResponseTime { get; set; }
    public double MaxResponseTimeTarget { get; set; }
    public int TotalIncidents { get; set; }
    public PerformanceMetrics Performance { get; set; } = new();
}