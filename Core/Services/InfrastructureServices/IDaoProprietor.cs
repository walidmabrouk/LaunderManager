using LaunderWebApi.Entities;

namespace LaunderManagerWebApi.Domain.Services.InfrastructureServices
{
    public interface IDaoProprietor
    {
        Task AddProprietor(Proprietor proprietor);
        Proprietor GetProprietorById(int id);
        IEnumerable<Proprietor> GetAllProprietors();
    }
}
