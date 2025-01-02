using Application.Interfaces;
using Domain.Repositories;
using Laundromat.Core.Interfaces;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMachineRepository _repository;
        private readonly IWebSocketService _webSocketService;

        public MachineService(IMachineRepository repository, IWebSocketService webSocketService)
        {
            _repository = repository;
            _webSocketService = webSocketService;
        }

        public async Task HandleMachineStartAsync(int machineId)
        {
            await _repository.UpdateMachineStateAsync(machineId, "Running");
            await _webSocketService.BroadcastMessageAsync($"Machine {machineId} has started.");
        }

        public async Task HandleMachineStopAsync(int machineId, decimal cyclePrice)
        {
            await _repository.AddCycleEarningsAsync(machineId, cyclePrice);
            await _repository.UpdateMachineStateAsync(machineId, "Stopped");
            await _webSocketService.BroadcastMessageAsync($"Machine {machineId} has stopped. Cycle price added: {cyclePrice:C}.");
        }
    }
}
