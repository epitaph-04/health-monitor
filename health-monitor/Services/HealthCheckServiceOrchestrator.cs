using health_monitor.Hub;
using health_monitor.Services;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.BackgroundServices;

public class HealthCheckServiceOrchestrator(
    IHubContext<NotificationHub, INotificationClient> context,
    IEnumerable<IHealthCheckService> healthCheckServices,
    StatusService statusService,
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
                var healthCheckResult =await healthCheckService.CheckHealthAsync();
            }
            logger.LogInformation("Executing health check {Time}", DateTime.Now);
            await context.Clients.All.ReceiveAllNotifications(statusService.GetServices());
        }
    }
}