using health_monitor.Models;

namespace health_monitor.Services.Metrics;

public interface IMetricsService
{
    Task<HealthMetrics> CalculateMetrics(string serviceId, TimeSpan period);
    Task<PerformanceMetrics> CalculatePerformanceMetrics(string serviceId, TimeSpan period);
    Task<double> CalculateAvailability(string serviceId, TimeSpan period);
    Task<TimeSpan> CalculateMeanTimeToRecovery(string serviceId, TimeSpan period);
    Task<TimeSpan> CalculateMeanTimeBetweenFailures(string serviceId, TimeSpan period);
    Task<SlaReport> GenerateSlaReport(string serviceId, SlaReportingPeriod period);
    Task<bool> CheckSlaCompliance(string serviceId);
}