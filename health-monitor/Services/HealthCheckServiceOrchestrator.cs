using health_monitor.Client.Model;
using health_monitor.Hub;
using health_monitor.Services;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.BackgroundServices;

public class HealthCheckServiceOrchestrator(
    IHubContext<NotificationHub, INotificationClient> context,
    IEnumerable<IHealthCheckService> healthCheckServices,
    ILogger<HealthCheckServiceOrchestrator> logger
    ) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var healthCheckService in healthCheckServices)
            {
                logger.LogInformation("Checking health for service: {ServiceName}", healthCheckService.Name);
                var healthCheckResult = await healthCheckService.CheckHealthAsync();
                var service = new Service
                {
                    Id = healthCheckService.Id,
                    Name = healthCheckService.Name,
                    Url = healthCheckService.Target,
                    ServiceType = healthCheckService.Type,
                    LastCheckStatus = new StatusInfo(
                        healthCheckResult.Status, 
                        healthCheckResult.Message, 
                        TimeOnly.FromDateTime(healthCheckResult.LastCheckedUtc), 
                        healthCheckResult.ResponseTime.Milliseconds),
                    HistoricStatus = new Queue<StatusInfo>(
                        healthCheckService.GetHistoricalHealthCheckResults()
                            .Select(h => new StatusInfo(h.Status, h.Message, TimeOnly.FromDateTime(h.LastCheckedUtc), h.ResponseTime.Milliseconds))
                        )
                };
                await context.Clients.All.ReceiveNotification(service);
            }
        }
    }
}