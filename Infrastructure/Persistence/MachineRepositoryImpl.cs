using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using LaunderManagerWebApi.Domain.InfrastructureServices;
using LaunderWebApi.Infrastructure.Dao;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence
{
    public class MachineRepositoryImpl : IMachineRepository
    {
        private readonly DbConnectionManager _connectionManager;
        private readonly ILogger<MachineRepositoryImpl> _logger;

        public MachineRepositoryImpl(DbConnectionManager connectionManager, ILogger<MachineRepositoryImpl> logger)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task UpdateMachineStateAsync(int machineId, string state)
        {
            const string query = "UPDATE Machines SET State = @State WHERE Id = @MachineId;";

            using var connection = _connectionManager.GetConnection();
            await ExecuteCommandAsync(connection, query, new { MachineId = machineId, State = state },
                $"Failed to update state for machine with ID {machineId}");
        }

        public async Task AddCycleEarningsAsync(int machineId, decimal price)
        {
            const string query = "UPDATE Machines SET Earnings = Earnings + @Price WHERE Id = @MachineId;";

            using var connection = _connectionManager.GetConnection();
            await ExecuteCommandAsync(connection, query, new { MachineId = machineId, Price = price },
                $"Failed to add cycle earnings for machine with ID {machineId}");
        }

        public async Task<string> GetMachineStateAsync(int machineId)
        {
            const string query = "SELECT State FROM Machines WHERE Id = @MachineId;";

            using var connection = _connectionManager.GetConnection();
            return await ExecuteQuerySingleAsync<string>(connection, query, new { MachineId = machineId },
                $"Failed to fetch state for machine with ID {machineId}");
        }

        public async Task<decimal> GetMachineEarningsAsync(int machineId)
        {
            const string query = "SELECT Earnings FROM Machines WHERE Id = @MachineId;";

            using var connection = _connectionManager.GetConnection();
            return await ExecuteQuerySingleAsync<decimal>(connection, query, new { MachineId = machineId },
                $"Failed to fetch earnings for machine with ID {machineId}");
        }

        private async Task ExecuteCommandAsync(IDbConnection connection, string query, object parameters, string errorMessage)
        {
            try
            {
                await connection.ExecuteAsync(query, parameters);
                _logger.LogInformation("Command executed successfully: {Query}", query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        private async Task<T> ExecuteQuerySingleAsync<T>(IDbConnection connection, string query, object parameters, string errorMessage)
        {
            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
                _logger.LogInformation("Query executed successfully: {Query}", query);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}
