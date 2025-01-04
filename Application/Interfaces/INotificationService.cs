using LaunderManagerWebApi.Domain.DTOs;
using System.Threading.Tasks;

namespace LaunderManagerWebApi.Application.Interfaces
{
    public interface INotificationService
    {
        public Task ProcessStateChangeAsync(MachineStatusDto notification);
    }
}
