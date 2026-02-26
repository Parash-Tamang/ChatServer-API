using MediatR;
using System.Text.Json.Serialization;
namespace AIChatbot.Application.RoleManagement.Queries;
public record ListRolesQuery(
    [property: JsonIgnore]
    string RequestedByUserId
) : IRequest<List<object>>;