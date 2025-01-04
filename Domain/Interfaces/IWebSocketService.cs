using System.Net.WebSockets;

namespace LaunderManagerWebApi.Domain.InfrastructureServices
{
    public interface IWebSocketService
    {
        public string AddSocket(WebSocket socket);
        Task SendMessageAsync(string connectionId, string message);
        Task BroadcastMessageAsync(string message);
    }
}
