namespace health_monitor.Models;

public class ServiceDependency
{
    public string ServiceId { get; set; } = null!;
    public string[] DependsOn { get; set; } = [];
    public DependencyType Type { get; set; } = DependencyType.Critical;
    public int Priority { get; set; } = 0; // Higher numbers = higher priority
}

public enum DependencyType
{
    Critical,    // Service fails if dependency fails
    Optional,    // Service degraded if dependency fails
    Circuit      // Circuit breaker pattern
}