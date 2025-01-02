using Laundromat.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

public class WebSocketService : IWebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public string AddSocket(WebSocket socket)
    {
        var connectionId = Guid.NewGuid().ToString();
        if (_connections.TryAdd(connectionId, socket))
        {
            Console.WriteLine($"Socket added with ID: {connectionId}");
        }
        else
        {
            Console.WriteLine($"Failed to add socket with ID: {connectionId}");
        }
        return connectionId;
    }



    public async Task SendMessageAsync(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var socket))
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        Console.WriteLine("Attempting to send broadcast message: " + message);
        foreach (var (connectionId, socket) in _connections)
        {
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine($"Message sent to {connectionId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending to {connectionId}: {ex.Message}");
                    _connections.TryRemove(connectionId, out _); // Clean up on error.
                }
            }
            else
            {
                Console.WriteLine($"Connection {connectionId} is closed, removing.");
                _connections.TryRemove(connectionId, out _); // Remove closed connections.
            }
        }
    }


}



