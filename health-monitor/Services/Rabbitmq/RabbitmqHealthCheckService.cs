using health_monitor.Client.Model;
using health_monitor.Models;

namespace health_monitor.Services;

public class RabbitmqHealthCheckService(ApplicationConfiguration appConfig) : IHealthCheckService
{
    public string Id => appConfig.Id;
    public string Name => appConfig.Name;
    public ServiceType Type => ServiceType.Rabbitmq;
    public string Target => appConfig.Target;
    public HealthCheckResult LastCheckedResult => new()
    {
        Message = "",
        ResponseTime = TimeSpan.Zero,
        Status = Status.Unknown,
        LastCheckedUtc = DateTime.UtcNow,
    };
    public Task<HealthCheckResult> CheckHealthAsync()
    {
        throw new NotImplementedException();
    }
    public IEnumerable<HealthCheckResult> GetHistoricalHealthCheckResults()
    {
        throw new NotImplementedException();
    }
}