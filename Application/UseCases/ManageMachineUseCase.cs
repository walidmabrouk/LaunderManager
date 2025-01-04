using LaunderManagerWebApi.Application.Interfaces;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;
using Laundromat.Core.Interfaces;

public class ManageMachineUseCase : IMachineService
{
    private readonly IMachineRepository _machineRepository;

    public ManageMachineUseCase(IMachineRepository machineRepository)
    {
        _machineRepository = machineRepository;
    }

    public async Task UpdateMachineStateAsync(int machineId, string state)
    {
        await _machineRepository.UpdateMachineStateAsync(machineId, state);
    }
    public async Task AddCycleEarningsAsync(int machineId, decimal price)
    {
        await _machineRepository.AddCycleEarningsAsync(machineId, price);
    }
}
