using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace AIChatbot.Application.Supersetup.Handlers;

public class SaveConnectionHandler
    : IRequestHandler<SaveConnectionCommand, SaveConnectionResult>
{
    private readonly IConnectionRepository _repo;

    public SaveConnectionHandler(IConnectionRepository repo)
    {
        _repo = repo;
    }

    public async Task<SaveConnectionResult> Handle(
        SaveConnectionCommand request,
        CancellationToken ct)
    {
        // =============================
        // 🔒 VALIDATION
        // =============================
        if (string.IsNullOrWhiteSpace(request.ServerName))
            throw new BadHttpRequestException("ServerName is required.");

        if (string.IsNullOrWhiteSpace(request.DatabaseName))
            throw new BadHttpRequestException("DatabaseName is required.");

        if (string.IsNullOrWhiteSpace(request.AuthMode))
            throw new BadHttpRequestException("AuthMode is required.");

        var connectionString = BuildConnectionString(request);

        // =============================
        // ✅ TEST CONNECTION
        // =============================
        await TestConnectionAsync(connectionString);

        // =============================
        // 🔒 DUPLICATE CHECK (FIXED POSITION)
        // =============================
        var existing = await _repo.GetByUniqueKeyAsync(
            request.ServerName,
            request.DatabaseName,
            request.AuthMode);

        if (existing != null)
        {
            if (request.Id == null)
            {
                throw new BadHttpRequestException(
                    "Connection already exists for this server and database.");
            }

            if (existing.Id != request.Id)
            {
                throw new BadHttpRequestException(
                    "Another connection with same details already exists.");
            }
        }

        ConnectionString entity;

        // =============================
        // ✅ CREATE
        // =============================
        if (request.Id == null)
        {
            entity = new ConnectionString
            {
                Id = Guid.NewGuid(),
                ServerName = request.ServerName,
                DatabaseName = request.DatabaseName,
                AuthMode = request.AuthMode,
                
                Username = IsWindowsAuth(request) ? null : request.Username,
                PasswordEncrypted = IsWindowsAuth(request) ? null : request.Password,
                ConnectionTimeout = request.ConnectionTimeout,
                TrustCertificate = request.TrustCertificate, 

                IsActive = false,
                Verified = false,
                PromptingMode = 0,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repo.AddAsync(entity);
        }
        // =============================
        // ✅ UPDATE
        // =============================
        else
        {
            entity = await _repo.GetByIdAsync(request.Id.Value)
                ?? throw new KeyNotFoundException("Connection not found.");

            entity.ServerName = request.ServerName;
            entity.DatabaseName = request.DatabaseName;
            entity.AuthMode = request.AuthMode;
            if (IsWindowsAuth(request))
            {
                entity.Username = null;
                entity.PasswordEncrypted = null;
            }
            else
            {
                entity.Username = request.Username;
                entity.PasswordEncrypted = request.Password;
            }
            entity.TrustCertificate = request.TrustCertificate;
            entity.ConnectionTimeout = request.ConnectionTimeout;

            entity.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(entity);
        }

        // =============================
        // ✅ FINAL RESPONSE
        // =============================
        return new SaveConnectionResult
        {
            Success = true,
            Message = "Connection tested and saved successfully.",
            ConnectionId = entity.Id
        };
    }

    // -------------------------------------------------------
    private async Task TestConnectionAsync(string connectionString)
    {
        using var conn = new SqlConnection(connectionString);

        try
        {
            await conn.OpenAsync();
        }
        catch (SqlException)
        {
            throw new BadHttpRequestException(
                "Unable to connect to database. Check server, credentials or database name.");
        }
    }

    // -------------------------------------------------------
    private string BuildConnectionString(SaveConnectionCommand request)
    {
        if (string.Equals(request.AuthMode, "windows", StringComparison.OrdinalIgnoreCase))
        {
            return $"Server={request.ServerName};Database={request.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";
        }

        return $"Server={request.ServerName};Database={request.DatabaseName};User Id={request.Username};Password={request.Password};TrustServerCertificate=True;";
    }
    private bool IsWindowsAuth(SaveConnectionCommand request)
    {
        return string.Equals(request.AuthMode, "windows", StringComparison.OrdinalIgnoreCase);
    }
}