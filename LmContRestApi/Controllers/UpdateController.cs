using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class UpdateController : ControllerBase
{
    private readonly string _connectionString;

    public UpdateController(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] JsonElement body)
    {
        var incomingKeys = new HashSet<string>(
            body.EnumerateObject().Select(p => p.Name),
            StringComparer.OrdinalIgnoreCase);

        var schema = await GetSchemaAsync();

        Console.WriteLine("Incoming keys: " + string.Join(", ", incomingKeys));
        foreach (var t in schema)
            Console.WriteLine($"Table {t.Key}: {string.Join(", ", t.Value)}");

        // Find matching tables: incoming keys must all exist as columns in the table
        var candidates = schema
            .Where(t => incomingKeys.IsSubsetOf(t.Value))
            .OrderBy(t => t.Value.Count) // prefer the tightest/most specific match
            .ToList();

        if (candidates.Count == 0)
            return BadRequest(new { error = "No table matches the given keys.", incomingKeys });

        var matchedTable = candidates[0].Key;

        await InsertAsync(matchedTable, body);

        return Ok(new { table = matchedTable, status = "inserted" });
    }

    private async Task<Dictionary<string, HashSet<string>>> GetSchemaAsync()
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string query = @"
        SELECT 
            TABLE_NAME, 
            COLUMN_NAME,
            COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IsIdentity
        FROM INFORMATION_SCHEMA.COLUMNS";

        using var cmd = new SqlCommand(query, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);
            var isIdentity = reader.GetInt32(2) == 1;

            if (isIdentity) continue; // skip auto-increment columns entirely

            if (!result.TryGetValue(table, out var cols))
                result[table] = cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            cols.Add(column);
        }

        return result;
    }

    private async Task InsertAsync(string table, JsonElement body)
    {
        var columns = new List<string>();
        var paramNames = new List<string>();

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand { Connection = conn };

        foreach (var prop in body.EnumerateObject())
        {
            columns.Add($"[{prop.Name}]");
            var paramName = $"@{prop.Name}";
            paramNames.Add(paramName);
            cmd.Parameters.AddWithValue(paramName, JsonElementToObject(prop.Value) ?? DBNull.Value);
        }

        cmd.CommandText =
            $"INSERT INTO [{table}] ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramNames)})";

        await cmd.ExecuteNonQueryAsync();
    }

    private static object? JsonElementToObject(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        JsonValueKind.Number => el.TryGetInt64(out var l) ? l : el.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => el.GetRawText()
    };
}