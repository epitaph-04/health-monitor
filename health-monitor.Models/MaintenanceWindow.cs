namespace health_monitor.Models;

public class MaintenanceWindow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ServiceId { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public MaintenanceType Type { get; set; } = MaintenanceType.Scheduled;
    public string Description { get; set; } = null!;
    public bool SuppressAlerts { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = "System";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum MaintenanceType
{
    Scheduled,
    Emergency,
    Planned,
    Unplanned
}