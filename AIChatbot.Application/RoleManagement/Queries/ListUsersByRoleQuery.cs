

using MediatR;
using System.Text.Json.Serialization;

namespace AIChatbot.Application.RoleManagement.Queries;
public record ListUsersByRoleQuery(
    string RoleId,

    [property: JsonIgnore]
    string RequestedByUserId
) : IRequest<List<object>>;