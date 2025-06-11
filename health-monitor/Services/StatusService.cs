using health_monitor.Client.Model;

namespace health_monitor.Services;

public class StatusService
{
    public Service[] GetServices()
    {
        var random = new Random();
        var db = new Service
            {
                Id = "db",
                Name = "DB",
                Url = new Uri("https://db/health/rediness"),
                ServiceType = ServiceType.Db,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [],
                HistoricStatus = new([
                    new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now.AddMinutes(-1)), random.Next(0, 100)),
                    new StatusInfo(Status.Degraded, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now.AddMinutes(-2)), random.Next(0, 100)),
                    new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now.AddMinutes(-3)), random.Next(0, 100)),
                ])
            };
            var sns = new Service
            {
                Id = "sns",
                Name = "SNS",
                Url = new Uri("https://sns/health/rediness"),
                ServiceType = ServiceType.MessageBus,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [],
                HistoricStatus = new()
            };
            var sqs = new Service
            {
                Id = "sqs",
                Name = "SQS",
                Url = new Uri("https://sqs/health/rediness"),
                ServiceType = ServiceType.MessageBus,
                LastCheckStatus = new StatusInfo(Status.Degraded, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [],
                HistoricStatus = new()
            };
            var maf = new Service
            {
                Id = "maf-notifier",
                Name = "Maf Notifier",
                Url = new Uri("https://maf-notifier/health/rediness"),
                ServiceType = ServiceType.Http,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [sqs],
                HistoricStatus = new()
            };
            var auditLogger = new Service
            {
                Id = "audit-logger",
                Name = "Audit Logger",
                Url = new Uri("https://audit-logger/health/rediness"),
                ServiceType = ServiceType.Http,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [db, sqs],
                HistoricStatus = new()
            };
            var cf = new Service
            {
                Id = "cf",
                Name = "Cf",
                Url = new Uri("https://cf/health/rediness"),
                ServiceType = ServiceType.Http,
                LastCheckStatus = new StatusInfo(Status.Healthy, "Ok - 200", TimeOnly.FromDateTime(DateTime.Now), random.Next(0, 100)),
                DependentServices = [db, sns],
                HistoricStatus = new()
            };
        return [db, sns, sqs, cf, auditLogger, maf];
    }
}