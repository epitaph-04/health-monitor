using health_monitor.Client.Model;
using health_monitor.Services;
using Microsoft.AspNetCore.SignalR;

namespace health_monitor.Hub;

public class NotificationHub(StatusService statusService) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).ReceiveAllNotification(statusService.GetServices());
        await base.OnConnectedAsync();
    }
}

public interface INotificationClient
{
    Task ReceiveAllNotification(Service[] service);
    Task ReceiveNotification(Service service);
}