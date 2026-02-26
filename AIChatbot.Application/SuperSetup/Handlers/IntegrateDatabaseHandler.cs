using AIChatbot.Application.Abstractions;
using AIChatbot.Application.SuperSetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Text;

namespace AIChatbot.Application.Supersetup.Handlers;

public class IntegrateDatabaseHandler : IRequestHandler<IntegrateDatabaseCommand, Guid>
{
    private readonly IConnectionRepository _connRepo;
    private readonly IPromptRepository _promptRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public IntegrateDatabaseHandler(
        IConnectionRepository connRepo,
        IPromptRepository promptRepo,
        UserManager<ApplicationUser> userManager)
    {
        _connRepo = connRepo;
        _promptRepo = promptRepo;
        _userManager = userManager;
    }

    public async Task<Guid> Handle(IntegrateDatabaseCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        //  validate requester
        var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
        if (requester == null)
            throw new Exception("Requester not found");

        var roles = await _userManager.GetRolesAsync(requester);

        if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            throw new UnauthorizedAccessException();

        // build SQL connection string
        string cs = dto.AuthMode.ToLower() == "windows"
            ? $"Server={dto.Server};Database={dto.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True"
            : $"Server={dto.Server};Database={dto.DatabaseName};User Id={dto.Username};Password={dto.Password};TrustServerCertificate=True";

        //  validate connection
        using (var conn = new SqlConnection(cs))
            await conn.OpenAsync(ct);

        // encrypt password
        var encryptedPassword = dto.Password == null
            ? null
            : Convert.ToBase64String(Encoding.UTF8.GetBytes(dto.Password));

        var db = new ConnectionString
        {
            ServerName = dto.Server,
            DatabaseName = dto.DatabaseName,
            AuthMode = dto.AuthMode,
            Username = dto.Username,
            PasswordEncrypted = encryptedPassword,
            DbIdentifier = dto.DbId,
            Role = dto.Role
        };

        await _connRepo.AddAsync(db);

        //  create ORIGINAL + CURRENT prompt sets
        var original = new PromptSet
        {
            ConnectionStringId = db.Id,
            VersionType = "ORIGINAL"
        };

        var current = new PromptSet
        {
            ConnectionStringId = db.Id,
            VersionType = "CURRENT"
        };

        await _promptRepo.AddPromptSetAsync(original);
        await _promptRepo.AddPromptSetAsync(current);

        // 🔥 default prompt functions
        await _promptRepo.ReplaceFunctionsAsync(original.Id, new List<PromptFunction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PromptSetId = original.Id,
                FunctionName = "construct_sql",
                SystemPrompt = "Generate SQL query from user intent."
            }
        });

        await _promptRepo.ReplaceFunctionsAsync(current.Id, new List<PromptFunction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PromptSetId = current.Id,
                FunctionName = "construct_sql",
                SystemPrompt = "Generate SQL query from user intent."
            }
        });

        return db.Id;
    }
}