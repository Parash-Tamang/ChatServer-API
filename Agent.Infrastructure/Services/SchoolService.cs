using Agent.Application.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace Agent.Infrastructure.Services
{
    public class SchoolService(IConfiguration configuration) : ISchoolService 
    {

        private readonly string _configurationString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Missing Connection String : ExternalDB");

        
        public async Task<String> ExecuteQueryAsync (String sql)
        {
            if (!Regex.IsMatch(sql, @"^\s*SELECT\s", RegexOptions.IgnoreCase))
                throw new InvalidOperationException("Only SELECT queries allowed.");

            if (sql.Contains(";")) // prevent multi-statement
                throw new InvalidOperationException("Multiple SQL statements not allowed.");

            using var conn = new SqlConnection(_configurationString);
            using var command = new SqlCommand(sql, conn);

            await conn.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object?>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return JsonSerializer.Serialize(results);            

        }

       
    }
}
