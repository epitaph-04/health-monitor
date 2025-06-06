using health_monitor.Client.Model;
using health_monitor.Hub;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.BackgroundServices;

public class HealthCheckService(
    IHubContext<NotificationHub, INotificationClient> context,
    StatusService statusService,
    ILogger<HealthCheckService> logger
    ) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Executing health check {Time}", DateTime.Now);
            await context.Clients.All.ReceiveNotification(statusService.GetServices());
        }
    }
}