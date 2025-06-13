using Microsoft.AspNetCore.SignalR;

namespace TaskAPI.Hubs
{
    public class TaskHub : Hub
    {
        public const string HUB_ENDPOINT = "/taskHub";
    }
}
