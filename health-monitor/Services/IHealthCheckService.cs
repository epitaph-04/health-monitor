using health_monitor.Models;

namespace health_monitor.Services;

public interface IHealthCheckService
{
    ApplicationType Type { get; }
    Task<HealthCheckResult> CheckHealthAsync();
}