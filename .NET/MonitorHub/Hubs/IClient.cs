using Microsoft.AspNetCore.SignalR;

namespace MonitorHub.Hubs
{
    public interface IClient :IClientProxy
    {
        string Value { get; set; }
    }
}
