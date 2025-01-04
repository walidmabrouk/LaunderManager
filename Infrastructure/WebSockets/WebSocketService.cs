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
        Console.WriteLine($"WebSocket added with ID: {connectionId}");
        return connectionId;
    }

    public async Task SendMessageAsync(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var socket))
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (WebSocketException ex)
                {
                    Console.Error.WriteLine($"Error sending message to WebSocket ID {connectionId}: {ex.Message}");
                    await RemoveSocketAsync(connectionId);
                }
            }
            else
            {
                await RemoveSocketAsync(connectionId);
            }
        }
        else
        {
            Console.Error.WriteLine($"WebSocket ID {connectionId} not found.");
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
                catch (WebSocketException ex)
                {
                    Console.Error.WriteLine($"Error broadcasting to WebSocket ID {connectionId}: {ex.Message}");
                    await RemoveSocketAsync(connectionId);
                }
            }
            else
            {
                await RemoveSocketAsync(connectionId);
            }
        }
    }

    private async Task RemoveSocketAsync(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var socket))
        {
            try
            {
                if (socket.State != WebSocketState.Closed)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }
            catch (WebSocketException ex)
            {
                Console.Error.WriteLine($"Error closing WebSocket ID {connectionId}: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"WebSocket ID {connectionId} removed.");
            }
        }
    }
}
