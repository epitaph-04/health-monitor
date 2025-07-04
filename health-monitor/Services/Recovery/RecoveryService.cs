using health_monitor.Models;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace health_monitor.Services.Recovery;

public class RecoveryService : IRecoveryService
{
    private readonly ConcurrentDictionary<string, List<RecoveryAction>> _recoveryActions = new();
    private readonly ConcurrentDictionary<string, List<RecoveryResult>> _recoveryHistory = new();
    private readonly ILogger<RecoveryService> _logger;

    public RecoveryService(ILogger<RecoveryService> logger)
    {
        _logger = logger;
        InitializeDefaultRecoveryActions();
    }

    public async Task<bool> CanExecuteRecovery(string serviceId, HealthCheckResult result)
    {
        if (result.Status != Status.Critical)
        {
            return false;
        }

        var actions = await GetAvailableRecoveryActions(serviceId);
        return actions.Any(a => a.IsEnabled && IsWithinCooldownPeriod(a) && a.ExecutionCount < a.MaxRetries);
    }

    public async Task<RecoveryResult> ExecuteRecovery(string serviceId, RecoveryAction action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new RecoveryResult
        {
            ServiceId = serviceId,
            ActionId = action.Id,
            ActionName = action.Name,
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Executing recovery action {ActionName} for service {ServiceId}", action.Name, serviceId);

            var success = await ExecuteRecoveryAction(action);
            
            result.Success = success;
            result.Message = success ? "Recovery action completed successfully" : "Recovery action failed";
            
            // Update action state
            action.LastExecuted = DateTime.UtcNow;
            action.ExecutionCount++;
            
            // Record recovery history
            RecordRecoveryResult(serviceId, result);
            
            _logger.LogInformation("Recovery action {ActionName} for service {ServiceId} completed with result: {Success}", 
                action.Name, serviceId, success);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Recovery action failed with exception: {ex.Message}";
            _logger.LogError(ex, "Recovery action {ActionName} for service {ServiceId} failed", action.Name, serviceId);
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        return result;
    }

    public Task<RecoveryAction[]> GetAvailableRecoveryActions(string serviceId)
    {
        if (_recoveryActions.TryGetValue(serviceId, out var actions))
        {
            return Task.FromResult(actions.Where(a => a.IsEnabled).ToArray());
        }
        
        return Task.FromResult(Array.Empty<RecoveryAction>());
    }

    public Task RegisterRecoveryAction(string serviceId, RecoveryAction action)
    {
        action.ServiceId = serviceId;
        
        var serviceActions = _recoveryActions.GetOrAdd(serviceId, _ => new List<RecoveryAction>());
        serviceActions.Add(action);
        
        _logger.LogInformation("Registered recovery action {ActionName} for service {ServiceId}", action.Name, serviceId);
        
        return Task.CompletedTask;
    }

    public Task<RecoveryResult[]> GetRecoveryHistory(string serviceId)
    {
        if (_recoveryHistory.TryGetValue(serviceId, out var history))
        {
            return Task.FromResult(history.OrderByDescending(r => r.ExecutedAt).ToArray());
        }
        
        return Task.FromResult(Array.Empty<RecoveryResult>());
    }

    private async Task<bool> ExecuteRecoveryAction(RecoveryAction action)
    {
        // Simulate recovery actions
        await Task.Delay(100); // Simulate recovery time
        
        return action.Type switch
        {
            RecoveryActionType.RestartService => SimulateServiceRestart(action),
            RecoveryActionType.ClearCache => SimulateCacheClear(action),
            RecoveryActionType.ScaleUp => SimulateScaleUp(action),
            RecoveryActionType.Failover => SimulateFailover(action),
            RecoveryActionType.CircuitBreakerReset => SimulateCircuitBreakerReset(action),
            RecoveryActionType.CustomScript => SimulateCustomScript(action),
            _ => false
        };
    }

    private bool SimulateServiceRestart(RecoveryAction action)
    {
        _logger.LogInformation("Simulating service restart for {ServiceId}", action.ServiceId);
        // In real implementation: call Kubernetes API, Docker API, or service management system
        return new Random().NextDouble() > 0.2; // 80% success rate
    }

    private bool SimulateCacheClear(RecoveryAction action)
    {
        _logger.LogInformation("Simulating cache clear for {ServiceId}", action.ServiceId);
        // In real implementation: call Redis FLUSHALL, clear application cache, etc.
        return new Random().NextDouble() > 0.1; // 90% success rate
    }

    private bool SimulateScaleUp(RecoveryAction action)
    {
        _logger.LogInformation("Simulating scale up for {ServiceId}", action.ServiceId);
        // In real implementation: call Kubernetes HPA, cloud auto-scaling APIs
        return new Random().NextDouble() > 0.3; // 70% success rate
    }

    private bool SimulateFailover(RecoveryAction action)
    {
        _logger.LogInformation("Simulating failover for {ServiceId}", action.ServiceId);
        // In real implementation: update load balancer, DNS records, service mesh configuration
        return new Random().NextDouble() > 0.25; // 75% success rate
    }

    private bool SimulateCircuitBreakerReset(RecoveryAction action)
    {
        _logger.LogInformation("Simulating circuit breaker reset for {ServiceId}", action.ServiceId);
        // In real implementation: reset circuit breaker state in service mesh or application
        return new Random().NextDouble() > 0.05; // 95% success rate
    }

    private bool SimulateCustomScript(RecoveryAction action)
    {
        _logger.LogInformation("Simulating custom script execution for {ServiceId}", action.ServiceId);
        // In real implementation: execute PowerShell, bash scripts, or custom automation
        return new Random().NextDouble() > 0.4; // 60% success rate
    }

    private bool IsWithinCooldownPeriod(RecoveryAction action)
    {
        if (action.LastExecuted == DateTime.MinValue)
        {
            return true; // Never executed
        }
        
        return DateTime.UtcNow - action.LastExecuted >= action.CooldownPeriod;
    }

    private void RecordRecoveryResult(string serviceId, RecoveryResult result)
    {
        var history = _recoveryHistory.GetOrAdd(serviceId, _ => new List<RecoveryResult>());
        history.Add(result);
        
        // Keep only last 100 recovery results per service
        if (history.Count > 100)
        {
            history.RemoveRange(0, history.Count - 100);
        }
    }

    private void InitializeDefaultRecoveryActions()
    {
        // These would typically be configured per service type or loaded from configuration
        var defaultActions = new[]
        {
            new RecoveryAction
            {
                Name = "Circuit Breaker Reset",
                Description = "Reset circuit breaker to allow new requests",
                Type = RecoveryActionType.CircuitBreakerReset,
                CooldownPeriod = TimeSpan.FromMinutes(2),
                MaxRetries = 5
            },
            new RecoveryAction
            {
                Name = "Cache Clear",
                Description = "Clear application cache to resolve stale data issues",
                Type = RecoveryActionType.ClearCache,
                CooldownPeriod = TimeSpan.FromMinutes(5),
                MaxRetries = 3
            },
            new RecoveryAction
            {
                Name = "Service Restart",
                Description = "Restart the service to recover from temporary failures",
                Type = RecoveryActionType.RestartService,
                CooldownPeriod = TimeSpan.FromMinutes(10),
                MaxRetries = 2
            }
        };

        // Register default actions for demo purposes
        // In real implementation, these would be configured per service
    }
}