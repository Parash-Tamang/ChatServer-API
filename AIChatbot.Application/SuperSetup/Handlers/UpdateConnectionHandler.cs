using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AIChatbot.Application.Supersetup.Handlers;

/// <summary>
/// Updates an existing DB connection
/// Only Admin / SuperAdmin allowed
/// </summary>
public class UpdateConnectionHandler
    : IRequestHandler<UpdateConnectionCommand, bool>
{
    private readonly IConnectionRepository _connectionRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateConnectionHandler(
        IConnectionRepository connectionRepo,
        UserManager<ApplicationUser> userManager)
    {
        _connectionRepo = connectionRepo;
        _userManager = userManager;
    }

    public async Task<bool> Handle(UpdateConnectionCommand request, CancellationToken ct)
    {
        // 🔐 validate requester
        var user = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (user == null)
            throw new Exception("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            throw new UnauthorizedAccessException("Only Admin/SuperAdmin can update DB");

        // 📦 fetch connection from repository
        var dbConn = await _connectionRepo.GetByIdAsync(request.DbId);

        if (dbConn == null)
            throw new Exception("Database connection not found");

        // 🔄 update fields
        dbConn.ServerName = request.Server;
        dbConn.DatabaseName = request.Database;
        dbConn.AuthMode = request.AuthMode;
        dbConn.Username = request.Username;
        dbConn.PasswordEncrypted = request.Password == null
            ? null
            : Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.Password));
        dbConn.TrustCertificate = request.TrustCertificate;
        dbConn.ConnectionTimeout = request.Timeout;

        await _connectionRepo.UpdateAsync(dbConn);

        return true;
    }
}