using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMachineService
    {
        public Task UpdateMachineStateAsync(int machineId, string state);
        public Task AddCycleEarningsAsync(int machineId, decimal price);
    }
}
