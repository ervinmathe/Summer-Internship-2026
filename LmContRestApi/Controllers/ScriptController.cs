using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LmContRestApi.Controllers
{
    [ApiController]
    [Route("api/scripts")]
    public class ScriptController : ControllerBase
    {
        private readonly string _connectionString;

        public ScriptController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing DefaultConnection connection string.");
        }

        // GET http://localhost:5153/api/scripts
        // Returns the most recently inserted .dll (highest Id)
        [HttpGet]
        public async Task<IActionResult> GetLatestDll()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = @"
                SELECT TOP 1 DllBytes
                FROM ScriptDlls
                ORDER BY Id DESC";

            using var cmd = new SqlCommand(query, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return NotFound();

            byte[] dllBytes = (byte[])reader["DllBytes"];
            return File(dllBytes, "application/octet-stream", "GeneratedScripts.dll");
        }

        // GET http://localhost:5153/api/scripts/version
        [HttpGet("version")]
        public async Task<IActionResult> GetLatestVersion()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = "SELECT TOP 1 Version FROM ScriptDlls ORDER BY Id DESC";
            using var cmd = new SqlCommand(query, conn);
            var version = await cmd.ExecuteScalarAsync() as string;

            return version is null ? NotFound() : Ok(version);
        }

        // POST http://localhost:5153/api/scripts
        // Inserts a new .dll as a new row
        [HttpPost]
        public async Task<IActionResult> InsertDll([FromBody] InsertDllRequest request)
        {
            if (request?.DllBytes is null || request.DllBytes.Length == 0)
                return BadRequest("No DLL bytes provided.");

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string query = @"
                INSERT INTO ScriptDlls (DllBytes, UploadedAtUtc)
                OUTPUT INSERTED.Id, INSERTED.UploadedAtUtc
                VALUES (@DllBytes, @UploadedAtUtc)";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@DllBytes", request.DllBytes);
            cmd.Parameters.AddWithValue("@UploadedAtUtc", DateTime.UtcNow);

            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            int id = reader.GetInt32(reader.GetOrdinal("Id"));
            DateTime uploadedAtUtc = reader.GetDateTime(reader.GetOrdinal("UploadedAtUtc"));

            return Ok(new { Id = id, UploadedAtUtc = uploadedAtUtc });
        }
    }

    public class InsertDllRequest
    {
        public byte[] DllBytes { get; set; } = Array.Empty<byte>();
    }
}