using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IRoleInstructionRepository
{
    Task<RoleInstruction?> GetByRoleIdAsync(string roleId);

    Task SaveAsync(RoleInstruction entity);

    Task UpdateAsync(RoleInstruction entity);
    Task DeleteAsync(string roleId);
}