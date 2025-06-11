using health_monitor.Client.Model;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.Hub;

public class NotificationHub : Hub<INotificationClient> { }

public interface INotificationClient
{
    Task ReceiveNotification(Service service);
}