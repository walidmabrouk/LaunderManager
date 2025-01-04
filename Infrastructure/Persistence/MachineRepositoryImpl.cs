using Dapper;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;
using System.Data;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class MachineRepository : IMachineRepository
    {
        private readonly DbConnectionManager _connectionManager;

        public MachineRepository(DbConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        /// <summary>
        /// Update the state of a machine.
        /// </summary>
        /// <param name="machineId">Machine ID.</param>
        /// <param name="state">New state of the machine.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task UpdateMachineStateAsync(int machineId, string state)
        {
            const string query = "UPDATE Machines SET State = @State WHERE Id = @MachineId;";

            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    await connection.ExecuteAsync(query, new { MachineId = machineId, State = state });
                    Console.WriteLine($"Machine state updated: Id={machineId}, State={state}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to update machine state: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Add cycle earnings to the machine's total earnings.
        /// </summary>
        /// <param name="machineId">Machine ID.</param>
        /// <param name="price">Price of the cycle.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddCycleEarningsAsync(int machineId, decimal price)
        {
            const string query = "UPDATE Machines SET Earnings = Earnings + @Price WHERE Id = @MachineId;";

            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    await connection.ExecuteAsync(query, new { MachineId = machineId, Price = price });
                    Console.WriteLine($"Cycle earnings added: MachineId={machineId}, Price={price}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to add cycle earnings: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetches the current state of a machine.
        /// </summary>
        /// <param name="machineId">Machine ID.</param>
        /// <returns>Returns the current state of the machine.</returns>
        public async Task<string> GetMachineStateAsync(int machineId)
        {
            const string query = "SELECT State FROM Machines WHERE Id = @MachineId;";

            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    var state = await connection.QueryFirstOrDefaultAsync<string>(query, new { MachineId = machineId });
                    Console.WriteLine($"Fetched machine state: Id={machineId}, State={state}");
                    return state;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to fetch machine state: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Fetches the total earnings for a machine.
        /// </summary>
        /// <param name="machineId">Machine ID.</param>
        /// <returns>Returns the total earnings of the machine.</returns>
        public async Task<decimal> GetMachineEarningsAsync(int machineId)
        {
            const string query = "SELECT Earnings FROM Machines WHERE Id = @MachineId;";

            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    var earnings = await connection.QueryFirstOrDefaultAsync<decimal>(query, new { MachineId = machineId });
                    Console.WriteLine($"Fetched machine earnings: Id={machineId}, Earnings={earnings}");
                    return earnings;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to fetch machine earnings: {ex.Message}");
                throw;
            }
        }
    }
}
