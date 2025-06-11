using health_monitor.Models;
using health_monitor.Services;

namespace health_monitor.Services;

public class SqsHealthCheckService(ApplicationConfiguration appConfig): IHealthCheckService
{
    public ApplicationType Type => ApplicationType.Sqs;
    public Task<HealthCheckResult> CheckHealthAsync()
    {
        throw new NotImplementedException();
    }
}