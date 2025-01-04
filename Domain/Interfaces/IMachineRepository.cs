using System.Threading.Tasks;

namespace LaunderManagerWebApi.Domain.InfrastructureServices
{
    public interface IMachineRepository
    {
        Task UpdateMachineStateAsync(int machineId, string state);
        Task AddCycleEarningsAsync(int machineId, decimal price);
        Task<string> GetMachineStateAsync(int machineId);
        Task<decimal> GetMachineEarningsAsync(int machineId);
    }
}
