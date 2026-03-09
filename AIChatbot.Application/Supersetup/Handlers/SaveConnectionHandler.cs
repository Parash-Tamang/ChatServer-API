using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.Data.SqlClient;

namespace AIChatbot.Application.Supersetup.Handlers;

public class SaveConnectionHandler
    : IRequestHandler<SaveConnectionCommand, List<ConnectionDto>>
{
    private readonly IConnectionRepository _repo;
    private readonly IAiProviderService _aiProvider;

    public SaveConnectionHandler(
        IConnectionRepository repo,
        IAiProviderService aiProvider)
    {
        _repo = repo;
        _aiProvider = aiProvider;
    }

    public async Task<List<ConnectionDto>> Handle(
        SaveConnectionCommand request,
        CancellationToken ct)
    {
        var connectionString = BuildConnectionString(request);

        // 1️⃣ Optional: Verify SQL connection
        // await TestConnectionAsync(connectionString);

        ConnectionString entity;

        if (request.Id == null)
        {
            entity = new ConnectionString
            {
                Id = Guid.NewGuid(),
                ServerName = request.ServerName,
                DatabaseName = request.DatabaseName,
                AuthMode = request.AuthMode,
                Username = request.Username,
                PasswordEncrypted = request.Password,
                TrustCertificate = request.TrustCertificate,
                ConnectionTimeout = request.ConnectionTimeout,
                IsActive = false,
                Verified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repo.AddAsync(entity);
        }
        else
        {
            entity = await _repo.GetByIdAsync(request.Id.Value)
                ?? throw new Exception("Connection not found");

            entity.ServerName = request.ServerName;
            entity.DatabaseName = request.DatabaseName;
            entity.AuthMode = request.AuthMode;
            entity.Username = request.Username;
            entity.PasswordEncrypted = request.Password;
            entity.TrustCertificate = request.TrustCertificate;
            entity.ConnectionTimeout = request.ConnectionTimeout;

            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
        }

        // 2️⃣ Ask AI Provider (Python) to verify & prepare DB
        var setupResult = await _aiProvider.PrepareDatabaseAsync(entity);

        entity.Verified = setupResult.DbStatus;

        await _repo.UpdateAsync(entity);

        // 3️⃣ Enforce activation rule
        if (request.IsActive && !entity.Verified)
        {
            throw new InvalidOperationException(
                "Connection cannot be activated until it is verified.");
        }

        // 4️⃣ Activate connection if allowed
        if (request.IsActive)
        {
            await _repo.SetActiveAsync(entity.Id);
        }

        // 5️⃣ Return all connections
        var connections = await _repo.GetAllAsync();

        return connections.Select(x => new ConnectionDto
        {
            Id = x.Id,
            ServerName = x.ServerName,
            DatabaseName = x.DatabaseName,
            AuthMode = x.AuthMode,
            IsActive = x.IsActive,
            Verified = x.Verified
        }).ToList();
    }

    // -------------------------------------------------------
    // TEST DATABASE CONNECTION
    // -------------------------------------------------------
    private async Task TestConnectionAsync(string connectionString)
    {
        using var conn = new SqlConnection(connectionString);

        try
        {
            await conn.OpenAsync();
        }
        catch (SqlException ex)
        {
            throw new Exception($"Database connection failed: {ex.Message}");
        }
    }

    // -------------------------------------------------------
    // BUILD SQL CONNECTION STRING
    // -------------------------------------------------------
    private string BuildConnectionString(SaveConnectionCommand request)
    {
        if (string.Equals(request.AuthMode, "windows", StringComparison.OrdinalIgnoreCase))
        {
            return $"Server={request.ServerName};Database={request.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
        }

        return $"Server={request.ServerName};Database={request.DatabaseName};User Id={request.Username};Password={request.Password};TrustServerCertificate=True;";
    }
}