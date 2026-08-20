using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface ISqlPermissionFilterBuilder
{
    SqlFilterResult Build(
        List<RoleRuntimePermission> permissions,

        RuntimeContextResult context,

        string schemaName,

        string tableName);
}