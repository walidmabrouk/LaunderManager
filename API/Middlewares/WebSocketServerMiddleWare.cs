using System.Net.WebSockets;
using System.Text;

public class WebSocketServerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly WebSocketService _connectionManager;

    public WebSocketServerMiddleware(RequestDelegate next, WebSocketService connectionManager)
    {
        _next = next;
        _connectionManager = connectionManager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            string connID = _connectionManager.AddSocket(webSocket);

            Console.WriteLine($"Socket connected with ID: {connID}");

            await SendMessageAsync(webSocket, $"Connected with ID: {connID}");

            await ReceiveMessageAsync(webSocket, async (result, message) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(message, 0, result.Count);
                    Console.WriteLine($"Message received from {connID}: {receivedMessage}");
                    await SendMessageAsync(webSocket, $"Echo: {receivedMessage}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"WebSocket {connID} is closing.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                }
            });
        }
        else
        {
            await _next(context);
        }
    }

    private async Task SendMessageAsync(WebSocket webSocket, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessageAsync(WebSocket webSocket, Func<WebSocketReceiveResult, byte[], Task> handleMessage)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            await handleMessage(result, buffer);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
        }
    }
}