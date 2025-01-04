using System.Threading.Tasks;
using LaunderWebApi.Entities;

public interface IConfigurationService
{
    Task SaveAndBroadcastConfigurationAsync(Proprietor configuration, RequiredServices services);
}
