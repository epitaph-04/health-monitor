namespace health_monitor.Models;

public class ServiceMetric
{
    public string ServiceId { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string ServiceType { get; set; } = "";
    public double Availability { get; set; }
    public double AvgResponseTime { get; set; }
    public double P95ResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public double HealthScore { get; set; }
}