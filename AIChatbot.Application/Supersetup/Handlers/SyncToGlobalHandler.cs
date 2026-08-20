using MediatR;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Abstractions;

namespace AIChatbot.Application.Supersetup.Handlers;

public class SyncToGlobalHandler : IRequestHandler<SyncToGlobalCommand, bool>
{
    private readonly ILocalFunctionRepository _localRepo;

    public SyncToGlobalHandler(ILocalFunctionRepository localRepo)
    {
        _localRepo = localRepo;
    }

    public async Task<bool> Handle(SyncToGlobalCommand request, CancellationToken ct)
    {
        var block = await _localRepo.GetAsync(
    request.ConnectionId,
    request.FunctionName);

        if (block == null)
        {
            // already global
            return true;
        }

        block.IsOverride = false;
        block.OverridePrompt = null;

        await _localRepo.UpdateAsync(block);

        return true;
    }
}