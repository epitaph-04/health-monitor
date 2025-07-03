using health_monitor.Client.Model;
using health_monitor.Hub;
using health_monitor.Services;
using health_monitor.Services.Alerting;
using health_monitor.Services.Dependencies;
using health_monitor.Models;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.BackgroundServices;

public class HealthCheckServiceOrchestrator(
    IHubContext<NotificationHub, INotificationClient> context,
    IEnumerable<IHealthCheckService> healthCheckServices,
    IAlertingService alertingService,
    IDependencyService dependencyService,
    ILogger<HealthCheckServiceOrchestrator> logger
    ) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var serviceStatuses = new Dictionary<string, Status>();
            var services = new List<Service>();

            // First pass: Execute health checks and collect statuses
            foreach (var healthCheckService in healthCheckServices)
            {
                logger.LogInformation("Checking health for service: {ServiceName}", healthCheckService.Name);
                var healthCheckResult = await healthCheckService.CheckHealthAsync();
                serviceStatuses[healthCheckService.Id] = healthCheckResult.Status;

                // Check if we should trigger an alert
                var shouldAlert = await alertingService.ShouldTriggerAlert(healthCheckService.Id, healthCheckResult);
                if (shouldAlert)
                {
                    var alertLevel = healthCheckResult.Status switch
                    {
                        Status.Critical => AlertLevel.Critical,
                        Status.Degraded => AlertLevel.Warning,
                        _ => AlertLevel.Info
                    };

                    await alertingService.SendAlert(alertLevel, healthCheckService.Id, healthCheckResult.Message, new Dictionary<string, object>
                    {
                        ["responseTime"] = healthCheckResult.ResponseTime.TotalMilliseconds,
                        ["lastChecked"] = healthCheckResult.LastCheckedUtc,
                        ["serviceType"] = healthCheckService.Type.ToString()
                    });
                }
            }

            // Second pass: Calculate dependency-aware statuses and create service objects
            foreach (var healthCheckService in healthCheckServices)
            {
                var baseStatus = serviceStatuses[healthCheckService.Id];
                var dependencyAwareStatus = await dependencyService.CalculateDependentStatus(
                    healthCheckService.Id, baseStatus, serviceStatuses);

                var service = new Service
                {
                    Id = healthCheckService.Id,
                    Name = healthCheckService.Name,
                    Url = healthCheckService.Target,
                    ServiceType = healthCheckService.Type,
                    Tag = healthCheckService.Tag,
                    LastCheckStatus = new StatusInfo(
                        dependencyAwareStatus, 
                        healthCheckService.LastCheckedResult.Message, 
                        TimeOnly.FromDateTime(healthCheckService.LastCheckedResult.LastCheckedUtc), 
                        healthCheckService.LastCheckedResult.ResponseTime.Milliseconds),
                    HistoricStatus = new Queue<StatusInfo>(
                        healthCheckService.GetHistoricalHealthCheckResults()
                            .Select(h => new StatusInfo(h.Status, h.Message, TimeOnly.FromDateTime(h.LastCheckedUtc), h.ResponseTime.Milliseconds))
                        )
                };

                // Add dependency information
                var dependencies = await dependencyService.GetDependenciesOf(healthCheckService.Id);
                service.DependentServices = dependencies.Select(depId => 
                    new Service { Id = depId, Name = depId }).ToList();

                services.Add(service);
                await context.Clients.All.ReceiveNotification(service);
            }
        }
    }
}