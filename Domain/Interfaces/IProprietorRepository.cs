using LaunderWebApi.Entities;
using System.Threading.Tasks;

public interface IDaoProprietor
{
    Task SaveProprietorAsync(Proprietor proprietor);
}
