using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories
{
    public class PromptFunctionRepository : IPromptFunctionRepository
    {
        private readonly AppDbContext _context;

        public PromptFunctionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PromptFunction>> GetByConnectionIdAsync(Guid connectionId)
        {
            return await _context.PromptFunctions
                .Where(x => x.ConnectionStringId == connectionId && !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<PromptFunction?> GetByIdAsync(Guid id)
        {
            return await _context.PromptFunctions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task AddAsync(PromptFunction entity)
        {
            _context.PromptFunctions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PromptFunction entity)
        {
            _context.PromptFunctions.Update(entity);
            await _context.SaveChangesAsync();
        }


        public async Task<List<PromptFunction>> GetGlobalFunctionsAsync()
        {
            return await _context.PromptFunctions
                .Where(x => x.ConnectionStringId == null && !x.IsDeleted)
                .ToListAsync();
        }
    }
}