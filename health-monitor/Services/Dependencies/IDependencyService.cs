using health_monitor.Models;
using health_monitor.Client.Model;

namespace health_monitor.Services.Dependencies;

public interface IDependencyService
{
    Task ConfigureDependencies(ServiceDependency[] dependencies);
    Task<ServiceDependency[]> GetDependencies();
    Task<string[]> GetDependentsOf(string serviceId);
    Task<string[]> GetDependenciesOf(string serviceId);
    Task<Status> CalculateDependentStatus(string serviceId, Status currentStatus, Dictionary<string, Status> dependencyStatuses);
    Task<DependencyGraph> BuildDependencyGraph();
    Task<string[]> FindRootCause(string failedServiceId);
}

public class DependencyGraph
{
    public Dictionary<string, DependencyNode> Nodes { get; set; } = new();
}

public class DependencyNode
{
    public string ServiceId { get; set; } = null!;
    public string[] Dependencies { get; set; } = [];
    public string[] Dependents { get; set; } = [];
    public DependencyType Type { get; set; }
    public Status CurrentStatus { get; set; }
}