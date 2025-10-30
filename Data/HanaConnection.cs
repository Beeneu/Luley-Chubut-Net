using System.Data.Odbc;

namespace Luley_Integracion_Net.Data;

public class HanaConnection(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public OdbcConnection CreateConnection()
    {
        return new OdbcConnection(_connectionString);
    }

    public async Task<OdbcConnection> CreateOpenConnectionAsync()
    {
        var connection = CreateConnection();
        await connection.OpenAsync();
        return connection;
    }
}

public static class HanaConnectionFactory
{
    public static HanaConnection Create(
        string host,
        string port,
        string database,
        string user,
        string password
    )
    {
        var connectionString =
            $"Driver={{HDBODBC}};ServerNode={host}:{port};Database={database};UID={user};PWD={password};CURRENTSCHEMA={database}";

        return new HanaConnection(connectionString);
    }
}
