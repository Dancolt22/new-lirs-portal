using System.Data;
using Microsoft.Data.SqlClient;

namespace LirsPortal.Api.Data;

public class DbConnectionFactory(IConfiguration config)
{
    public IDbConnection Create() => new SqlConnection(config.GetConnectionString("LirsDb"));
}
