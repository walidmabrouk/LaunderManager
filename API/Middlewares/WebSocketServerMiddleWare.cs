using LaunderManagerWebApi.Application.Interfaces;
using LaunderManagerWebApi.Domain.DTOs;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Entities;
using LaunderWebApi.Infrastructure.Dao;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;

public record RequiredServices(
    IWebSocketService WebSocketService,
    INotificationService NotificationService,
    IConfigurationService ConfigurationService);


public sealed class WebSocketServerMiddleware : IAsyncDisposable
{
    private const int BUFFER_SIZE = 4 * 1024; // 4KB buffer
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebSocketServerMiddleware> _logger;

    public WebSocketServerMiddleware(
        RequestDelegate next,
        IServiceProvider serviceProvider,
        ILogger<WebSocketServerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            await _next(context);
            return;
        }

        await HandleWebSocketConnectionAsync(context);
    }

    private async Task HandleWebSocketConnectionAsync(HttpContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = GetRequiredServices(scope);

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = services.WebSocketService.AddSocket(socket);

        try
        {
            await ListenToWebSocketAsync(socket, connectionId, services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket connection");
            await CloseSocketWithErrorAsync(socket, "Internal server error occurred");
        }
    }

    private async Task ListenToWebSocketAsync(
        WebSocket socket,
        string connectionId,
        RequiredServices services)
    {
        var buffer = new byte[BUFFER_SIZE];
        var receiveResult = await socket.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None);

        while (socket.State == WebSocketState.Open && !receiveResult.CloseStatus.HasValue)
        {
            if (receiveResult.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                await ProcessWebSocketMessageAsync(message, socket, services);
            }

            receiveResult = await socket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);
        }
    }

    private async Task ProcessWebSocketMessageAsync(
        string message,
        WebSocket socket,
        RequiredServices services)
    {
        try
        {
            var messageType = GetMessageType(message);
            if (messageType == null)
            {
                await CloseSocketWithErrorAsync(socket, "Invalid message format");
                return;
            }

            await HandleMessageByTypeAsync(messageType, message, services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
            await CloseSocketWithErrorAsync(socket, "Error processing message");
        }
    }

    private static BaseMessageDto? GetMessageType(string message)
    {
        try
        {
            return JsonSerializer.Deserialize<BaseMessageDto>(message);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task HandleMessageByTypeAsync(
        BaseMessageDto messageType,
        string rawMessage,
        RequiredServices services)
    {
        switch (messageType.Type?.ToUpperInvariant())
        {
            case "NOTIFICATION":
                await HandleMachineNotificationAsync(rawMessage, services);
                break;

            case "CONFIGURATION":
                await HandleProprietorConfigurationAsync(rawMessage, services);
                break;

            default:
                _logger.LogWarning("Unknown message type received: {Type}", messageType.Type);
                break;
        }
    }

    private async Task HandleMachineNotificationAsync(
        string message,
        RequiredServices services)
    {
        var notification = JsonSerializer.Deserialize<MachineStatusDto>(message);
        if (notification == null) return;

        await services.NotificationService.ProcessStateChangeAsync(notification);
    }

    private async Task HandleProprietorConfigurationAsync(
        string message,
        RequiredServices services)
    {
        var configuration = JsonSerializer.Deserialize<Proprietor>(message);
        if (configuration == null) return;

        await services.ConfigurationService.SaveAndBroadcastConfigurationAsync(configuration, services);
    }

    private static async Task CloseSocketWithErrorAsync(
        WebSocket socket,
        string message)
    {
        if (socket.State == WebSocketState.Open)
        {
            await socket.CloseAsync(
                WebSocketCloseStatus.InvalidPayloadData,
                message,
                CancellationToken.None);
        }
    }

    private RequiredServices GetRequiredServices(IServiceScope scope)
    {
        var provider = scope.ServiceProvider;
        return new RequiredServices(
            provider.GetRequiredService<IWebSocketService>(),
            provider.GetRequiredService<INotificationService>(),
            provider.GetRequiredService<IConfigurationService>());
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup code if needed
    }
}
