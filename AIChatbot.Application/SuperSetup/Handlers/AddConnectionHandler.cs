using AIChatbot.Application.Abstractions;
using AIChatbot.Application.SuperSetup.Commands;
using AIChatbot.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace AIChatbot.Application.SuperSetup.Handlers;

public class AddConnectionHandler : IRequestHandler<AddConnectionCommand, Guid>
{
    private readonly IConnectionRepository _connectionRepo;
    private readonly IPromptRepository _promptRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public AddConnectionHandler(
        IConnectionRepository connectionRepo,
        IPromptRepository promptRepo,
        UserManager<ApplicationUser> userManager)
    {
        _connectionRepo = connectionRepo;
        _promptRepo = promptRepo;
        _userManager = userManager;
    }

    public async Task<Guid> Handle(AddConnectionCommand request, CancellationToken ct)
    {
        var requester = await _userManager.FindByIdAsync(request.RequestedByUserId);
        var roles = await _userManager.GetRolesAsync(requester!);

        if (!roles.Contains("SuperAdmin") && !roles.Contains("Admin"))
            throw new UnauthorizedAccessException("Only SA/Admin can add DB connections");

        var connection = new ConnectionString
        {
            ServerName = request.Server,
            DatabaseName = request.DatabaseName,
            AuthMode = request.AuthMode,
            Username = request.Username,
            PasswordEncrypted = request.Password, // encrypt later
            TrustCertificate = request.TrustCertificate,
            ConnectionTimeout = request.ConnectionTimeout,
            DbIdentifier = request.DbIdentifier
        };

        await _connectionRepo.AddAsync(connection);

        var connectionId = connection.Id;

        var promptSet = new PromptSet
        {
            ConnectionStringId = connectionId,
            VersionType = "ORIGINAL",
            VersionNumber = 1,
            CreatedBy = request.RequestedByUserId
        };

        await _promptRepo.AddPromptSetAsync(promptSet);

        var functions = request.PromptFunctions
    .Select(f => new PromptFunction
    {
        Id = Guid.NewGuid(),
        PromptSetId = promptSet.Id,
        FunctionName = f.FunctionName,
        SystemPrompt = f.SystemPrompt
    })
    .ToList();

        await _promptRepo.AddFunctionsAsync(functions);

        return connectionId;
    }
}