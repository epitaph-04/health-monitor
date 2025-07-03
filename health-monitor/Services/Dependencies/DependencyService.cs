using health_monitor.Models;
using health_monitor.Client.Model;
using System.Collections.Concurrent;

namespace health_monitor.Services.Dependencies;

public class DependencyService : IDependencyService
{
    private readonly ConcurrentDictionary<string, ServiceDependency> _dependencies = new();
    private readonly ILogger<DependencyService> _logger;

    public DependencyService(ILogger<DependencyService> logger)
    {
        _logger = logger;
    }

    public Task ConfigureDependencies(ServiceDependency[] dependencies)
    {
        _dependencies.Clear();
        foreach (var dependency in dependencies)
        {
            _dependencies[dependency.ServiceId] = dependency;
        }
        _logger.LogInformation("Configured {Count} service dependencies", dependencies.Length);
        return Task.CompletedTask;
    }

    public Task<ServiceDependency[]> GetDependencies()
    {
        return Task.FromResult(_dependencies.Values.ToArray());
    }

    public Task<string[]> GetDependentsOf(string serviceId)
    {
        var dependents = _dependencies.Values
            .Where(d => d.DependsOn.Contains(serviceId))
            .Select(d => d.ServiceId)
            .ToArray();
        
        return Task.FromResult(dependents);
    }

    public Task<string[]> GetDependenciesOf(string serviceId)
    {
        if (_dependencies.TryGetValue(serviceId, out var dependency))
        {
            return Task.FromResult(dependency.DependsOn);
        }
        
        return Task.FromResult(Array.Empty<string>());
    }

    public Task<Status> CalculateDependentStatus(string serviceId, Status currentStatus, Dictionary<string, Status> dependencyStatuses)
    {
        if (!_dependencies.TryGetValue(serviceId, out var dependency))
        {
            return Task.FromResult(currentStatus);
        }

        var worstDependencyStatus = Status.Healthy;
        
        foreach (var dependencyId in dependency.DependsOn)
        {
            if (dependencyStatuses.TryGetValue(dependencyId, out var depStatus))
            {
                switch (dependency.Type)
                {
                    case DependencyType.Critical:
                        if (depStatus == Status.Critical)
                            return Task.FromResult(Status.Critical);
                        if (depStatus == Status.Degraded && worstDependencyStatus < Status.Degraded)
                            worstDependencyStatus = Status.Degraded;
                        break;
                        
                    case DependencyType.Optional:
                        if (depStatus == Status.Critical && worstDependencyStatus < Status.Degraded)
                            worstDependencyStatus = Status.Degraded;
                        break;
                        
                    case DependencyType.Circuit:
                        // Circuit breaker logic - service can continue if dependency fails
                        // but mark as degraded if dependency is down for too long
                        if (depStatus == Status.Critical && worstDependencyStatus < Status.Degraded)
                            worstDependencyStatus = Status.Degraded;
                        break;
                }
            }
        }

        // Return the worse of current status or dependency-derived status
        return Task.FromResult((Status)Math.Max((int)currentStatus, (int)worstDependencyStatus));
    }

    public Task<DependencyGraph> BuildDependencyGraph()
    {
        var graph = new DependencyGraph();
        
        // First, create nodes for all services
        var allServiceIds = _dependencies.Values
            .SelectMany(d => d.DependsOn.Concat(new[] { d.ServiceId }))
            .Distinct()
            .ToArray();

        foreach (var serviceId in allServiceIds)
        {
            graph.Nodes[serviceId] = new DependencyNode
            {
                ServiceId = serviceId,
                Dependencies = Array.Empty<string>(),
                Dependents = Array.Empty<string>(),
                Type = DependencyType.Critical,
                CurrentStatus = Status.Unknown
            };
        }

        // Then, populate dependencies and dependents
        foreach (var dependency in _dependencies.Values)
        {
            if (graph.Nodes.TryGetValue(dependency.ServiceId, out var node))
            {
                node.Dependencies = dependency.DependsOn;
                node.Type = dependency.Type;
                
                // Add this service as a dependent to all its dependencies
                foreach (var depId in dependency.DependsOn)
                {
                    if (graph.Nodes.TryGetValue(depId, out var depNode))
                    {
                        var dependents = depNode.Dependents.ToList();
                        if (!dependents.Contains(dependency.ServiceId))
                        {
                            dependents.Add(dependency.ServiceId);
                            depNode.Dependents = dependents.ToArray();
                        }
                    }
                }
            }
        }

        return Task.FromResult(graph);
    }

    public async Task<string[]> FindRootCause(string failedServiceId)
    {
        var rootCauses = new List<string>();
        var visited = new HashSet<string>();
        
        await FindRootCauseRecursive(failedServiceId, rootCauses, visited);
        
        return rootCauses.Distinct().ToArray();
    }

    private async Task FindRootCauseRecursive(string serviceId, List<string> rootCauses, HashSet<string> visited)
    {
        if (visited.Contains(serviceId))
            return;

        visited.Add(serviceId);
        
        var dependencies = await GetDependenciesOf(serviceId);
        
        if (dependencies.Length == 0)
        {
            // This is a root service (has no dependencies)
            rootCauses.Add(serviceId);
            return;
        }

        foreach (var dependencyId in dependencies)
        {
            await FindRootCauseRecursive(dependencyId, rootCauses, visited);
        }
    }
}