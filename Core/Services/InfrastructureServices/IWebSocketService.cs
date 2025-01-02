using System.Net.WebSockets;

namespace Laundromat.Core.Interfaces
{
    public interface IWebSocketService
    {
        public string AddSocket(WebSocket socket);
        Task SendMessageAsync(string connectionId, string message);
        Task BroadcastMessageAsync(string message);
    }
}
