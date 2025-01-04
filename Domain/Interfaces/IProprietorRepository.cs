using System.Collections.Generic;
using System.Threading.Tasks;
using LaunderWebApi.Entities;

namespace LaunderWebApi.Infrastructure.Dao
{
    public interface IProprietorRepository
    {
        Task<IEnumerable<Proprietor>> GetAllProprietors();
        Proprietor GetProprietorById(int id);
        Task<int> AddProprietor(Proprietor proprietor);
    }
}
