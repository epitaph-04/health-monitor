using health_monitor.Models;

namespace health_monitor.Services.Configuration;

public interface IConfigurationManager
{
    Task UpdateServiceConfiguration(string serviceId, ApplicationConfiguration config);
    Task<bool> ValidateConfiguration(ApplicationConfiguration config);
    Task ReloadConfiguration();
    Task<ApplicationConfiguration[]> GetAllConfigurations();
    Task<ApplicationConfiguration?> GetConfiguration(string serviceId);
    Task DeleteConfiguration(string serviceId);
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
}

public class ConfigurationChangedEventArgs : EventArgs
{
    public string ServiceId { get; set; } = null!;
    public ConfigurationChangeType ChangeType { get; set; }
    public ApplicationConfiguration? Configuration { get; set; }
}

public enum ConfigurationChangeType
{
    Added,
    Updated,
    Deleted,
    Reloaded
}