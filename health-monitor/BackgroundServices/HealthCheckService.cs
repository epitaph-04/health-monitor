using health_monitor.Client.Model;
using health_monitor.Hub;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.BackgroundServices;

public class HealthCheckService(
    IHubContext<NotificationHub, INotificationClient> context,
    ILogger<HealthCheckService> logger) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            logger.LogInformation("Executing health check {Time}", DateTime.Now);
            
            var db = new Service
            {
                Id = "db",
                Name = "DB",
                Url = new Uri("https://db/health/rediness"),
                ServiceType = ServiceType.Db,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [],
                HistoricStatus = []
            };
            var sns = new Service
            {
                Id = "sns",
                Name = "SNS",
                Url = new Uri("https://sns/health/rediness"),
                ServiceType = ServiceType.MessageBus,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [],
                HistoricStatus = []
            };
            var sqs = new Service
            {
                Id = "sqs",
                Name = "SQS",
                Url = new Uri("https://sqs/health/rediness"),
                ServiceType = ServiceType.MessageBus,
                LastCheckStatus = new StatusInfo(Status.Degraded, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [],
                HistoricStatus = []
            };
            var maf = new Service
            {
                Id = "maf-notifier",
                Name = "Maf Notifier",
                Url = new Uri("https://maf-notifier/health/rediness"),
                ServiceType = ServiceType.Http,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [sqs],
                HistoricStatus = []
            };
            var auditLogger = new Service
            {
                Id = "audit-logger",
                Name = "Audit Logger",
                Url = new Uri("https://audit-logger/health/rediness"),
                ServiceType = ServiceType.Http,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [db, sqs],
                HistoricStatus = []
            };
            var cf = new Service
            {
                Id = "cf",
                Name = "Cf",
                Url = new Uri("https://cf/health/rediness"),
                ServiceType = ServiceType.Http,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [db, sns],
                HistoricStatus = []
            };
            await context.Clients.All.ReceiveNotification([db, sns, sqs, cf, auditLogger, maf]);
        }
    }
}