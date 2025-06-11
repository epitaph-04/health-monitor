using health_monitor.Models;

namespace health_monitor.Services;

public class RabbitmqHealthCheckService(ApplicationConfiguration appConfig) : IHealthCheckService
{
    public ApplicationType Type => ApplicationType.Rabbitmq;
    public Task<HealthCheckResult> CheckHealthAsync()
    {
        throw new NotImplementedException();
    }
}