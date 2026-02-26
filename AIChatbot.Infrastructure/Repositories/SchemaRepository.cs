using AIChatbot.Application.Abstractions;
using AIChatbot.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class SchemaRepository : ISchemaRepository
{
    private readonly AppDbContext _db;

    public SchemaRepository(AppDbContext db)
    {
        _db = db;
    }
    private string? Decrypt(string? encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
            return null;

        var bytes = Convert.FromBase64String(encrypted);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
    public async Task<Dictionary<string, List<string>>> GetSchemaAsync(Guid connectionId)
    {
        var connEntity = await _db.ConnectionStrings.FindAsync(connectionId);

        if (connEntity == null)
            throw new Exception("Connection not found");

        string connectionString =
      connEntity.AuthMode.ToLower() == "windows"
          ? $"Server={connEntity.ServerName};Database={connEntity.DatabaseName};Trusted_Connection=True;TrustServerCertificate={connEntity.TrustCertificate};Connection Timeout={connEntity.ConnectionTimeout}"
          : $"Server={connEntity.ServerName};Database={connEntity.DatabaseName};User Id={connEntity.Username};Password={Decrypt(connEntity.PasswordEncrypted)};TrustServerCertificate={connEntity.TrustCertificate};Connection Timeout={connEntity.ConnectionTimeout}";

        var schema = new Dictionary<string, List<string>>();

        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT TABLE_NAME, COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
        ", conn);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var table = reader.GetString(0);
            var column = reader.GetString(1);

            if (!schema.ContainsKey(table))
                schema[table] = new List<string>();

            schema[table].Add(column);
        }

        return schema;
    }
}