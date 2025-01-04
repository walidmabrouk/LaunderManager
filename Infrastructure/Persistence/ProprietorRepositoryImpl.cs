using System.Data;
using Dapper;
using LaunderWebApi.Entities;

namespace LaunderWebApi.Infrastructure.Dao
{
    public class ProprietorRepositoryImpl : IProprietorRepository
    {
        private readonly DbConnectionManager _connectionManager;

        public ProprietorRepositoryImpl(DbConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task<IEnumerable<Proprietor>> GetAllProprietors()
        {
            using var connection = _connectionManager.GetConnection();

            const string query = @"
        SELECT * FROM Proprietors;
        SELECT * FROM Laundries;
        SELECT * FROM Machines;
        SELECT * FROM Cycles;
    ";

            using var multi = await connection.QueryMultipleAsync(query);

            var proprietors = (await multi.ReadAsync<Proprietor>()).ToList();
            var laundries = (await multi.ReadAsync<Laundry>()).ToList();
            var machines = (await multi.ReadAsync<Machine>()).ToList();
            var cycles = (await multi.ReadAsync<Cycle>()).ToList();

            foreach (var proprietor in proprietors)
            {
                proprietor.Laundries = laundries.Where(l => l.ProprietorId == proprietor.Id).ToList();
                foreach (var laundry in proprietor.Laundries)
                {
                    laundry.Machines = machines.Where(m => m.LaundryId == laundry.Id).ToList();
                    foreach (var machine in laundry.Machines)
                    {
                        machine.Cycles = cycles.Where(c => c.MachineId == machine.Id).ToList();
                    }
                }
            }

            return proprietors;
        }

        public Proprietor GetProprietorById(int id)
        {
            using var connection = _connectionManager.GetConnection();

            const string query = @"
                SELECT * FROM Proprietors WHERE Id = @Id;
                SELECT * FROM Laundries WHERE ProprietorId = @Id;
                SELECT * FROM Machines WHERE LaundryId IN (SELECT Id FROM Laundries WHERE ProprietorId = @Id);
                SELECT * FROM Cycles WHERE MachineId IN (SELECT Id FROM Machines WHERE LaundryId IN (SELECT Id FROM Laundries WHERE ProprietorId = @Id));
            ";

            using var multi = connection.QueryMultiple(query, new { Id = id });

            var proprietor = multi.ReadFirstOrDefault<Proprietor>();
            if (proprietor == null) return null;

            var laundries = multi.Read<Laundry>().ToList();
            var machines = multi.Read<Machine>().ToList();
            var cycles = multi.Read<Cycle>().ToList();

            proprietor.Laundries = laundries;
            foreach (var laundry in laundries)
            {
                laundry.Machines = machines.Where(m => m.LaundryId == laundry.Id).ToList();
                foreach (var machine in laundry.Machines)
                {
                    machine.Cycles = cycles.Where(c => c.MachineId == machine.Id).ToList();
                }
            }

            return proprietor;
        }

        public async Task<int> AddProprietor(Proprietor proprietor)
        {
            using var connection = _connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string proprietorQuery = @"
            INSERT INTO Proprietors (Name, Email, TotalEarnings)
            VALUES (@Name, @Email, @TotalEarnings);
            SELECT CAST(SCOPE_IDENTITY() as int);
        ";

                var proprietorId = await connection.QuerySingleAsync<int>(proprietorQuery, proprietor, transaction);

                // Process laundries, machines, and cycles here...

                transaction.Commit();
                return proprietorId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

    }
}
