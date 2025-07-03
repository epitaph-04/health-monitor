using health_monitor.Models;

namespace health_monitor.Services.Maintenance;

public interface IMaintenanceService
{
    Task<MaintenanceWindow> CreateMaintenanceWindow(MaintenanceWindow window);
    Task<MaintenanceWindow[]> GetMaintenanceWindows(string? serviceId = null);
    Task<MaintenanceWindow?> GetMaintenanceWindow(string windowId);
    Task<bool> IsServiceInMaintenance(string serviceId);
    Task<MaintenanceWindow?> GetCurrentMaintenanceWindow(string serviceId);
    Task UpdateMaintenanceWindow(MaintenanceWindow window);
    Task DeleteMaintenanceWindow(string windowId);
    Task<MaintenanceWindow[]> GetActiveMaintenanceWindows();
}