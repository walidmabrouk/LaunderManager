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
            // Désérialiser le message reçu
            var data = JsonSerializer.Deserialize<MachineStateDto>(message);

            if (data != null)
            {
                // Vérifier le type de message
                if (data.State == "Running")
                {
                    // Démarrage de la machine
                    await machineService.UpdateMachineStateAsync(data.MachineId, "Running");
                    await webSocketService.BroadcastMessageAsync($"Machine {data.MachineId} started");
                    Console.WriteLine($"Machine {data.MachineId} started, state set to Running.");
                }
                else if (data.State == "Stopped")
                {
                    // Arrêt de la machine (ajouter les gains)
                    await machineService.UpdateMachineStateAsync(data.MachineId, "Stopped");

                    // Si vous avez un champ "price" dans l'objet reçu, vous pouvez l'utiliser
                    if (data.Price.HasValue)
                    {
                        await machineService.AddCycleEarningsAsync(data.MachineId, data.Price.Value);
                        await webSocketService.BroadcastMessageAsync($"Machine {data.MachineId} stopped, earnings added.");
                        Console.WriteLine($"Machine {data.MachineId} stopped, added earnings: {data.Price}");
                    }
                    else
                    {
                        // Cas où le prix n'est pas passé (vous pouvez définir un comportement par défaut ici)
                        await webSocketService.BroadcastMessageAsync($"Machine {data.MachineId} stopped, no earnings specified.");
                        Console.WriteLine($"Machine {data.MachineId} stopped, no earnings specified.");
                    }
                }
                else
                {
                    await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid state", CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Error processing message", CancellationToken.None);
            Console.Error.WriteLine($"Error processing WebSocket message: {ex.Message}");
        }
    }


}
