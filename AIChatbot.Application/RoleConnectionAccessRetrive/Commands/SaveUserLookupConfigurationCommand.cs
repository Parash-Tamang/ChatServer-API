using AIChatbot.Application.Common;
using MediatR;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Commands;

public record SaveUserLookupConfigurationCommand(
    string RoleId,
    Guid ConnectionId,
    string UserTableName,
    string UserIdColumn,
    string? EmailColumn,
    string? PhoneColumn
) : IRequest<GenericResult>;