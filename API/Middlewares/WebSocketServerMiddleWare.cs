using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Application.Interfaces;
using LaunderWebApi.Entities;
using Laundromat.Core.Interfaces;

public class WebSocketServerMiddleWare
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public WebSocketServerMiddleWare(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var webSocketService = scope.ServiceProvider.GetRequiredService<IWebSocketService>();
                var machineService = scope.ServiceProvider.GetRequiredService<IMachineService>();

                var socket = await context.WebSockets.AcceptWebSocketAsync();
                string connectionId = webSocketService.AddSocket(socket);

                try
                {
                    await ListenWebSocketAsync(socket, connectionId, webSocketService, machineService);
                }
                catch
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error occurred", CancellationToken.None);
                }
            }
        }
        else
        {
            await _next(context);
        }
    }

    private async Task ListenWebSocketAsync(WebSocket socket, string connectionId, IWebSocketService webSocketService, IMachineService machineService)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message, socket, connectionId, webSocketService, machineService);
            }
        }
    }

    private async Task ProcessMessageAsync(string message, WebSocket socket, string connectionId, IWebSocketService webSocketService, IMachineService machineService)
    {
        try
        {
            Console.WriteLine($"[ProcessMessageAsync] Received raw message: {message}");

            var data = JsonSerializer.Deserialize<MachineStateDto>(message);

            if (data == null || data.MachineId <= 0 || string.IsNullOrEmpty(data.State))
            {
                Console.WriteLine("[ProcessMessageAsync] Invalid deserialized data.");
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid message format", CancellationToken.None);
                return;
            }

            Console.WriteLine($"[ProcessMessageAsync] Valid data: MachineId={data.MachineId}, State={data.State}");
            await machineService.UpdateMachineStateAsync(data.MachineId, data.State);

            await webSocketService.BroadcastMessageAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessMessageAsync] Exception: {ex.Message}");
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Error processing message", CancellationToken.None);
        }
    }

}
