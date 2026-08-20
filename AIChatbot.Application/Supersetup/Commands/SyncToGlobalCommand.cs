using MediatR;

namespace AIChatbot.Application.Supersetup.Commands;

public class SyncToGlobalCommand : IRequest<bool>
{
    public Guid ConnectionId { get; set; }
    public string FunctionName { get; set; } = string.Empty;
}