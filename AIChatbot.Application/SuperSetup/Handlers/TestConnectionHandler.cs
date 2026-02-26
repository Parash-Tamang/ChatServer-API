using AIChatbot.Application.DTOs;
using AIChatbot.Application.SuperSetup.Commands;
using MediatR;
using Microsoft.Data.SqlClient;

public class TestConnectionHandler
    : IRequestHandler<TestConnectionCommand, bool>
{
    public async Task<bool> Handle(TestConnectionCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        string connectionString;

        if (dto.AuthMode.ToLower() == "windows")
        {
            connectionString =
                $"Server={dto.Server};Database={dto.DatabaseName};Trusted_Connection=True;TrustServerCertificate={dto.TrustCertificate};Connection Timeout={dto.ConnectionTimeout}";
        }
        else
        {
            connectionString =
                $"Server={dto.Server};Database={dto.DatabaseName};User Id={dto.Username};Password={dto.Password};TrustServerCertificate={dto.TrustCertificate};Connection Timeout={dto.ConnectionTimeout}";
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}