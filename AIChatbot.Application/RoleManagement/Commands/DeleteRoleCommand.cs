using MediatR;
using System.Text.Json.Serialization;
namespace AIChatbot.Application.RoleManagement.Commands;

public record DeleteRoleCommand(
    string RoleId,

    [property: JsonIgnore]
    string RequestedByUserId
) : IRequest<bool>;