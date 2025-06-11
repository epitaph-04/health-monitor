using health_monitor.Client.Model;
using health_monitor.Models;

namespace health_monitor.Services;

public interface IHealthCheckService
{
    string Id { get; }
    string Name { get; }
    ServiceType Type { get; }
    string Target { get; }
    HealthCheckResult LastCheckedResult { get; }
    Task<HealthCheckResult> CheckHealthAsync();
    IEnumerable<HealthCheckResult> GetHistoricalHealthCheckResults();
}