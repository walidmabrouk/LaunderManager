using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMachineService
    {
        Task HandleMachineStartAsync(int machineId);
        Task HandleMachineStopAsync(int machineId, decimal cyclePrice);
    }
}
