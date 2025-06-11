using health_monitor.Client.Model;
using health_monitor.Models;
using health_monitor.Services;

namespace health_monitor.Services;

public class SnsHealthCheckService(ApplicationConfiguration appConfig): IHealthCheckService
{
    public string Id => appConfig.Id;
    public string Name => appConfig.Name;
    public ServiceType Type => ServiceType.Sns;
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