using System.Threading.Tasks;

namespace LaunderManagerWebApi.Domain.InfrastructureServices
{
    public interface IWebSocketService
    {
        string AddSocket(System.Net.WebSockets.WebSocket socket);
        Task SendMessageAsync(string connectionId, string message);
        Task BroadcastMessageAsync(string message);
    }
}
