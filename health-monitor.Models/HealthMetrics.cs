namespace health_monitor.Models;

public class HealthMetrics
{
    public string ServiceId { get; set; } = null!;
    public double AvailabilityPercentage { get; set; }
    public TimeSpan MeanTimeToRecovery { get; set; }
    public TimeSpan MeanTimeBetweenFailures { get; set; }
    public TimeSpan TotalDowntime { get; set; }
    public PerformanceMetrics Performance { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan CalculationPeriod { get; set; } = TimeSpan.FromDays(30);
}

public class PerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public double P50ResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double P99ResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public int TotalRequests { get; set; }
    public int FailedRequests { get; set; }
}