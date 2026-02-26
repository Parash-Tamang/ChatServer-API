using MediatR;
using System.Text.Json.Serialization;
namespace AIChatbot.Application.RoleManagement.Commands;
public record CreateAdminCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,

    [property: JsonIgnore]
    string RequestedByUserId
) : IRequest<bool>;