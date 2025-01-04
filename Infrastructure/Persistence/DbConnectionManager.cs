using System.Data.SqlClient;
using System.Data;

public class DbConnectionManager : IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<DbConnectionManager> _logger;
    private IDbConnection? _connection;

    public DbConnectionManager(string connectionString, ILogger<DbConnectionManager> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDbConnection GetConnection()
    {
        if (_connection is { State: ConnectionState.Open })
        {
            _logger.LogWarning("An existing database connection is already open.");
            return _connection;
        }

        try
        {
            _connection = new SqlConnection(_connectionString); // Fixed syntax
            _connection.Open();
            _logger.LogInformation("Successfully connected to the database.");
            return _connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to the database.");
            throw new InvalidOperationException("Failed to connect to the database.", ex);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_connection is { State: ConnectionState.Open })
            {
                _connection.Close();
                _logger.LogInformation("Database connection closed.");
            }
            _connection?.Dispose();
        }
    }
}