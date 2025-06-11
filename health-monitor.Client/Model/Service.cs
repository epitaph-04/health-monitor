namespace health_monitor.Client.Model;

public class Service
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Url { get; init; } = null!;
    public ServiceType ServiceType { get; init; } = ServiceType.Http;
    public StatusInfo LastCheckStatus { get; init; } = null!;
    public Queue<StatusInfo> HistoricStatus { get; init; } = new(5);
    public List<Service> DependentServices { get; init; } = new();
}