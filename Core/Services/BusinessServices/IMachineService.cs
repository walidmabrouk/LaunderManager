using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMachineService
    {
        public Task UpdateMachineStateAsync(int machineId, string state);
    }
}
