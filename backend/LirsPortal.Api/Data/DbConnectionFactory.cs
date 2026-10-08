using System.Data;
using Microsoft.Data.SqlClient;

namespace LirsPortal.Api.Data;

/// <summary>
/// Centralized factory for creating SQL Server database connections.
/// 
/// Senior Engineer Note for Beginners:
/// Instead of hardcoding connection strings in multiple repositories,
/// this factory injects the configuration once and produces standardized IDbConnection
/// instances on demand. This makes it effortless to point to a test database or change
/// credentials in appsettings.json without touching repository code.
/// </summary>
public class DbConnectionFactory(IConfiguration config)
{
    /// <summary>
    /// Creates a new openable connection to the configured SQL Server instance.
    /// </summary>
    public IDbConnection Create() => new SqlConnection(config.GetConnectionString("LirsDb"));
}
