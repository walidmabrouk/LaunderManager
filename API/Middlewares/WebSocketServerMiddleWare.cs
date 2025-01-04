using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using Application.Interfaces;
using LaunderWebApi.Entities;
using Laundromat.Core.Interfaces;
using LaunderWebApi.Infrastructure.Dao;
using LaunderManagerWebApi.Domain.Services.InfrastructureServices;

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
                var proprietorDao = scope.ServiceProvider.GetRequiredService<IDaoProprietor>();

                var socket = await context.WebSockets.AcceptWebSocketAsync();
                string connectionId = webSocketService.AddSocket(socket);

                try
                {
                    await ListenWebSocketAsync(socket, connectionId, webSocketService, machineService, proprietorDao);
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

    private async Task ListenWebSocketAsync(
        WebSocket socket,
        string connectionId,
        IWebSocketService webSocketService,
        IMachineService machineService,
        IDaoProprietor proprietorDao)
    {
        var buffer = new byte[1024 * 4];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await ProcessMessageAsync(message, socket, connectionId, webSocketService, machineService, proprietorDao);
            }
        }
    }

    private async Task ProcessMessageAsync(string message, WebSocket socket, string connectionId, IWebSocketService webSocketService, IMachineService machineService, IDaoProprietor proprietorDao)
    {
        try
        {
            // Désérialiser le message reçu dans un objet générique
            var baseMessage = JsonSerializer.Deserialize<BaseMessageDto>(message);

            if (baseMessage == null || string.IsNullOrEmpty(baseMessage.Type))
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid message format", CancellationToken.None);
                return;
            }

            // Gestion en fonction du type de message
            if (baseMessage.Type == "Notification")
            {
                // Désérialisation en MachineStateDto pour une notification
                var notification = JsonSerializer.Deserialize<MachineStateDto>(message);

                if (notification != null)
                {
                    await HandleNotificationAsync(notification, webSocketService, machineService);
                }
            }
            else if (baseMessage.Type == "Configuration")
            {
                // Désérialisation en ProprietorDto pour une configuration
                var configuration = JsonSerializer.Deserialize<Proprietor>(message);

                if (configuration != null)
                {
                    await HandleConfigurationAsync(configuration, proprietorDao, webSocketService);
                }
            }
            else
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Unknown message type", CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Error processing message", CancellationToken.None);
            Console.Error.WriteLine($"Error processing WebSocket message: {ex.Message}");
        }
    }

    // Gestion des notifications
    private async Task HandleNotificationAsync(MachineStateDto notification, IWebSocketService webSocketService, IMachineService machineService)
    {
        if (notification.State == "Running")
        {
            // Démarrage de la machine
            await machineService.UpdateMachineStateAsync(notification.MachineId, "Running");
            await webSocketService.BroadcastMessageAsync($"Machine {notification.MachineId} started");
            Console.WriteLine($"Machine {notification.MachineId} started, state set to Running.");
        }
        else if (notification.State == "Stopped")
        {
            // Arrêt de la machine (ajouter les gains)
            await machineService.UpdateMachineStateAsync(notification.MachineId, "Stopped");

            if (notification.Price.HasValue)
            {
                await machineService.AddCycleEarningsAsync(notification.MachineId, notification.Price.Value);
                await webSocketService.BroadcastMessageAsync($"Machine {notification.MachineId} stopped, earnings added.");
                Console.WriteLine($"Machine {notification.MachineId} stopped, added earnings: {notification.Price}");
            }
            else
            {
                await webSocketService.BroadcastMessageAsync($"Machine {notification.MachineId} stopped, no earnings specified.");
                Console.WriteLine($"Machine {notification.MachineId} stopped, no earnings specified.");
            }
        }
        else
        {
            Console.WriteLine($"Invalid state for notification: {notification.State}");
        }
    }

    // Gestion des configurations
    private async Task HandleConfigurationAsync(Proprietor configuration, IDaoProprietor proprietorDao, IWebSocketService webSocketService)
    {
        try
        {
            // Sauvegarde de la configuration dans la base de données via DAO
            await proprietorDao.AddProprietor(configuration);

            // Diffusion de la confirmation via WebSocket
            Console.WriteLine($"Configuration saved for proprietor: {configuration.Name}");
            await webSocketService.BroadcastMessageAsync($"Configuration saved for proprietor: {configuration.Name}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving configuration: {ex.Message}");
            throw;
        }
    }
}
