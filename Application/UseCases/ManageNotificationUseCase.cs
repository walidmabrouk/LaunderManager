using LaunderManagerWebApi.Application.Interfaces;
using LaunderManagerWebApi.Domain.DTOs;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;

public class ManageNotificationUseCase : INotificationService
{
    private readonly IMachineRepository _machineRepository;
    private readonly IWebSocketService _webSocketService;
    private readonly IProprietorRepository _proprietorRepository;

    public ManageNotificationUseCase(IMachineRepository machineRepository, IWebSocketService webSocketService, IProprietorRepository proprietorRepository)
    {
        _machineRepository = machineRepository ?? throw new ArgumentNullException(nameof(machineRepository));
        _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
        _proprietorRepository = proprietorRepository;
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
        var Data = await _proprietorRepository.GetAllProprietors();
        await _webSocketService.BroadcastMessageAsync(
System.Text.Json.JsonSerializer.Serialize(Data));
    }

    // Handle machine stopped event
    private async Task HandleMachineStoppedAsync(MachineStatusDto notification)
    {
        await _machineRepository.UpdateMachineStateAsync(notification.MachineId, "Stopped");

        if (notification.Price.HasValue)
        {
            await _machineRepository.AddCycleEarningsAsync(notification.MachineId, notification.Price.Value);
            var Data = await _proprietorRepository.GetAllProprietors();
            await _webSocketService.BroadcastMessageAsync(
    System.Text.Json.JsonSerializer.Serialize(Data));
        }
    }
}
