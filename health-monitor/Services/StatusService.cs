using health_monitor.Client.Model;

namespace health_monitor.Services;

public class StatusService(IEnumerable<IHealthCheckService> services)
{
    public Service[] GetServices()
    {
        return services.Select(s => new Service
        {
            Id = s.Id,
            Name = s.Name,
            Url = s.Target,
            ServiceType = s.Type,
            LastCheckStatus = new StatusInfo(
                s.LastCheckedResult.Status, 
                s.LastCheckedResult.Message, 
                TimeOnly.FromDateTime(s.LastCheckedResult.LastCheckedUtc), 
                s.LastCheckedResult.ResponseTime.Milliseconds),
            HistoricStatus = new Queue<StatusInfo>(
                s.GetHistoricalHealthCheckResults()
                    .Select(h => new StatusInfo(h.Status, h.Message, TimeOnly.FromDateTime(h.LastCheckedUtc), h.ResponseTime.Milliseconds))
                )
        }).ToArray();
    }
}