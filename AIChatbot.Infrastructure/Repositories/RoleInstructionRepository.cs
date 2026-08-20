using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories;

public class RoleInstructionRepository
    : IRoleInstructionRepository
{
    private readonly AppDbContext _context;

    public RoleInstructionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RoleInstruction?> GetByRoleIdAsync(string roleId)
    {
        return await _context.RoleInstructions
            .FirstOrDefaultAsync(x => x.RoleId == roleId);
    }

    public async Task SaveAsync(RoleInstruction entity)
    {
        _context.RoleInstructions.Add(entity);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RoleInstruction entity)
    {
        _context.RoleInstructions.Update(entity);

        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(string roleId)
    {
        var existing = await _context.RoleInstructions
            .FirstOrDefaultAsync(x => x.RoleId == roleId);

        if (existing == null)
            return;

        _context.RoleInstructions.Remove(existing);

        await _context.SaveChangesAsync();
    }
}