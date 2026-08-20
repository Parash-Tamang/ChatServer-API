namespace AIChatbot.Application.Abstractions;

public interface IRuntimeViewService
{
    Task<(string ViewName, string ViewSql)>
     CreateViewAsync(
         string roleId,
        Guid connectionId);

    Task DeleteViewAsync(
        Guid connectionId,
        string viewName);
}