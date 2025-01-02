using Application.Interfaces;
using Domain.Repositories;
using Laundromat.Core.Interfaces;

public class MachineService : IMachineService
{
    private readonly IMachineRepository _machineRepository;

    public MachineService(IMachineRepository machineRepository)
    {
        _machineRepository = machineRepository;
    }

    public async Task UpdateMachineStateAsync(int machineId, string state)
    {
        await _machineRepository.UpdateMachineStateAsync(machineId, state);
    }
}
