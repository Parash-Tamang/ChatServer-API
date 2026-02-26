using AIChatbot.Application.RoleAccess.Models;
namespace AIChatbot.Application.Abstractions;

public interface IRoleAccessService
{
    Task<RoleAccessResult> GetAccessAsync(string roleName);
}