using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IMachineRepository
    {
        Task UpdateMachineStateAsync(int machineId, string state);
        Task AddCycleEarningsAsync(int machineId, decimal price);
    }
}
