using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRuntimeViewQueryBuilder
{
    string Build(
        string viewName,

        List<RoleRuntimePermission> permissions);
}