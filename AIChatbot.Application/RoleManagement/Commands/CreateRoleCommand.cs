using MediatR;
using System.Text.Json.Serialization;
namespace AIChatbot.Application.RoleManagement.Commands;

public record CreateRoleCommand(
    string RoleName,

    [property: JsonIgnore]
    string RequestedByUserId
) : IRequest<bool>;