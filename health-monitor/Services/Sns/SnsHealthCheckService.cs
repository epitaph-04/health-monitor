using health_monitor.Models;
using health_monitor.Services;

namespace health_monitor.Services;

public class SnsHealthCheckService(ApplicationConfiguration appConfig): IHealthCheckService
{
    public ApplicationType Type => ApplicationType.Sns;
    public Task<HealthCheckResult> CheckHealthAsync()
    {
        throw new NotImplementedException();
    }
}