using MediatR;
using System.Text.Json.Serialization;
namespace AIChatbot.Application.RoleManagement.Commands;

public record DeleteUserCommand(
    string TargetUserId,

    [property: JsonIgnore]
    string RequestedByUserId
) : IRequest<bool>;