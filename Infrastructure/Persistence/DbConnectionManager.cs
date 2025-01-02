using System;
using System.Data;
using System.Data.SqlClient;

namespace LaunderWebApi.Infrastructure.Dao
{
    public class DbConnectionManager
    {
        private readonly string _connectionString;

        public DbConnectionManager(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        // Create and open a new database connection
        public IDbConnection GetConnection()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                Console.WriteLine("connected to database");
                return connection;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to connect to the database.", ex);
            }
        }
    }
}
