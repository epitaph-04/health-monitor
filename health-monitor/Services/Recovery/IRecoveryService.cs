using health_monitor.Models;

namespace health_monitor.Services.Recovery;

public interface IRecoveryService
{
    Task<bool> CanExecuteRecovery(string serviceId, HealthCheckResult result);
    Task<RecoveryResult> ExecuteRecovery(string serviceId, RecoveryAction action);
    Task<RecoveryAction[]> GetAvailableRecoveryActions(string serviceId);
    Task RegisterRecoveryAction(string serviceId, RecoveryAction action);
    Task<RecoveryResult[]> GetRecoveryHistory(string serviceId);
}

public interface IRecoveryAction
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    RecoveryActionType Type { get; }
    TimeSpan CooldownPeriod { get; }
    int MaxRetries { get; }
    Task<bool> CanExecute(string serviceId, HealthCheckResult result);
    Task<RecoveryResult> Execute(string serviceId, Dictionary<string, object>? parameters = null);
}

public class RecoveryAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ServiceId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public RecoveryActionType Type { get; set; }
    public TimeSpan CooldownPeriod { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime LastExecuted { get; set; } = DateTime.MinValue;
    public int ExecutionCount { get; set; } = 0;
}

public enum RecoveryActionType
{
    RestartService,
    ClearCache,
    ScaleUp,
    Failover,
    CustomScript,
    CircuitBreakerReset
}

public class RecoveryResult
{
    public string ServiceId { get; set; } = null!;
    public string ActionId { get; set; } = null!;
    public string ActionName { get; set; } = null!;
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}