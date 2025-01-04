using LaunderWebApi.Entities;

public interface IConfigurationService
{
    Task<int> AddConfigurationAsync(Proprietor configuration);
    Task<IEnumerable<Proprietor>> GetAllConfigurationsAsync();
}