using System.Data.Odbc;
using Luley_Integracion_Net.Data;

namespace Luley_Integracion_Net.Repositories;

public class OrderRepository(HanaConnection hanaConnection)
{
    private readonly HanaConnection _hanaConnection = hanaConnection;
    private readonly string QUERY_PATH = "Queries/UpdateOrders.sql";

    public async Task<List<Dictionary<string, object>>> GetOrdersToUpdateAsync()
    {
        var results = new List<Dictionary<string, object>>();
        string query = File.ReadAllText(QUERY_PATH);

        await using var connection = await _hanaConnection.CreateOpenConnectionAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = query;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            results.Add(row);
        }

        return results;
    }
}
