using LaunderManagerWebApi.Application.Interfaces;
using LaunderManagerWebApi.Domain.DTOs;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;

public class ManageNotificationUseCase : INotificationService
{
    private readonly IMachineRepository _machineRepository;
    private readonly IWebSocketService _webSocketService;

    public ManageNotificationUseCase(IMachineRepository machineRepository, IWebSocketService webSocketService)
    {
        _machineRepository = machineRepository ?? throw new ArgumentNullException(nameof(machineRepository));
        _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
    }

    // Main entry point for processing state changes
    public async Task ProcessStateChangeAsync(MachineStatusDto notification)
    {
        if (string.IsNullOrEmpty(notification.State)) return;

        switch (notification.State.ToUpperInvariant())
        {
            case "RUNNING":
                await HandleMachineStartedAsync(notification);
                break;

            case "STOPPED":
                await HandleMachineStoppedAsync(notification);
                break;

            default:
                // Log or handle unexpected states
                break;
        }
    }

    // Handle machine started event
    private async Task HandleMachineStartedAsync(MachineStatusDto notification)
    {
        await _machineRepository.UpdateMachineStateAsync(notification.MachineId, "Running");

        await _webSocketService.BroadcastMessageAsync($"Machine {notification.MachineId} started");
    }

    // Handle machine stopped event
    private async Task HandleMachineStoppedAsync(MachineStatusDto notification)
    {
        await _machineRepository.UpdateMachineStateAsync(notification.MachineId, "Stopped");

        if (notification.Price.HasValue)
        {
            await _machineRepository.AddCycleEarningsAsync(notification.MachineId, notification.Price.Value);

            await _webSocketService.BroadcastMessageAsync($"Machine {notification.MachineId} stopped, earnings added");
        }
        else
        {
            await _webSocketService.BroadcastMessageAsync($"Machine {notification.MachineId} stopped, no earnings specified");
        }
    }
}
