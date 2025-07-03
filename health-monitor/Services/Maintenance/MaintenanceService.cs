using health_monitor.Models;
using System.Collections.Concurrent;

namespace health_monitor.Services.Maintenance;

public class MaintenanceService : IMaintenanceService
{
    private readonly ConcurrentDictionary<string, MaintenanceWindow> _maintenanceWindows = new();
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(ILogger<MaintenanceService> logger)
    {
        _logger = logger;
    }

    public Task<MaintenanceWindow> CreateMaintenanceWindow(MaintenanceWindow window)
    {
        window.Id = Guid.NewGuid().ToString();
        window.CreatedAt = DateTime.UtcNow;
        
        _maintenanceWindows[window.Id] = window;
        _logger.LogInformation("Created maintenance window {WindowId} for service {ServiceId}", window.Id, window.ServiceId);
        
        return Task.FromResult(window);
    }

    public Task<MaintenanceWindow[]> GetMaintenanceWindows(string? serviceId = null)
    {
        var windows = _maintenanceWindows.Values.AsEnumerable();
        
        if (!string.IsNullOrEmpty(serviceId))
        {
            windows = windows.Where(w => w.ServiceId == serviceId);
        }
        
        return Task.FromResult(windows.OrderBy(w => w.StartTime).ToArray());
    }

    public Task<MaintenanceWindow?> GetMaintenanceWindow(string windowId)
    {
        _maintenanceWindows.TryGetValue(windowId, out var window);
        return Task.FromResult(window);
    }

    public Task<bool> IsServiceInMaintenance(string serviceId)
    {
        var now = DateTime.UtcNow;
        var isInMaintenance = _maintenanceWindows.Values.Any(w => 
            w.ServiceId == serviceId && 
            w.IsActive && 
            w.StartTime <= now && 
            w.EndTime >= now);
        
        return Task.FromResult(isInMaintenance);
    }

    public Task<MaintenanceWindow?> GetCurrentMaintenanceWindow(string serviceId)
    {
        var now = DateTime.UtcNow;
        var window = _maintenanceWindows.Values.FirstOrDefault(w => 
            w.ServiceId == serviceId && 
            w.IsActive && 
            w.StartTime <= now && 
            w.EndTime >= now);
        
        return Task.FromResult(window);
    }

    public Task UpdateMaintenanceWindow(MaintenanceWindow window)
    {
        if (_maintenanceWindows.ContainsKey(window.Id))
        {
            _maintenanceWindows[window.Id] = window;
            _logger.LogInformation("Updated maintenance window {WindowId}", window.Id);
        }
        else
        {
            throw new ArgumentException($"Maintenance window {window.Id} not found");
        }
        
        return Task.CompletedTask;
    }

    public Task DeleteMaintenanceWindow(string windowId)
    {
        if (_maintenanceWindows.TryRemove(windowId, out var window))
        {
            _logger.LogInformation("Deleted maintenance window {WindowId} for service {ServiceId}", windowId, window.ServiceId);
        }
        else
        {
            throw new ArgumentException($"Maintenance window {windowId} not found");
        }
        
        return Task.CompletedTask;
    }

    public Task<MaintenanceWindow[]> GetActiveMaintenanceWindows()
    {
        var now = DateTime.UtcNow;
        var activeWindows = _maintenanceWindows.Values
            .Where(w => w.IsActive && w.StartTime <= now && w.EndTime >= now)
            .OrderBy(w => w.StartTime)
            .ToArray();
        
        return Task.FromResult(activeWindows);
    }
}