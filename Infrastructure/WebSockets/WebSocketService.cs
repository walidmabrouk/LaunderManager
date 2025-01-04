using LaunderManagerWebApi.Domain.InfrastructureServices;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

public class WebSocketService : IWebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public string AddSocket(WebSocket socket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, socket);
        return connectionId;
    }

    public async Task SendMessageAsync(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var socket) && socket.State == WebSocketState.Open)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);

        foreach (var (connectionId, socket) in _connections)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    _connections.TryRemove(connectionId, out _);
                }
            }
            else
            {
                _connections.TryRemove(connectionId, out _);
            }
        }
    }
}
