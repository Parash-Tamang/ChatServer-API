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

        // =============================
        // ✅ GET BY CONNECTION
        // =============================
        public async Task<List<PromptFunction>> GetByConnectionIdAsync(Guid connectionId)
        {
            return await _context.PromptFunctions
                .Where(x => x.ConnectionStringId == connectionId && !x.IsDeleted)
                .AsNoTracking() // 🔥 PERFORMANCE IMPROVEMENT
                .ToListAsync();
        }

        // =============================
        // ✅ GET BY ID
        // =============================
        public async Task<PromptFunction?> GetByIdAsync(Guid id)
        {
            return await _context.PromptFunctions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        // =============================
        // ✅ ADD
        // =============================
        public async Task AddAsync(PromptFunction entity)
        {
            entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
            entity.IsDeleted = false;

            _context.PromptFunctions.Add(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<PromptFunction?> GetByNameAsync(string functionName)
        {
            return await _context.PromptFunctions
                .FirstOrDefaultAsync(x =>
                    x.FunctionName == functionName &&
                    !x.IsDeleted);
        }

        // =============================
        // ✅ UPDATE
        // =============================
        public async Task UpdateAsync(PromptFunction entity)
        {
            var existing = await _context.PromptFunctions
                .FirstOrDefaultAsync(x => x.Id == entity.Id && !x.IsDeleted);

            if (existing == null)
                throw new Exception("Function not found or already deleted");

            // 🔥 Controlled update (avoid overwriting unintended fields)
            existing.FunctionName = entity.FunctionName;
            existing.SystemPrompt = entity.SystemPrompt;

            // IMPORTANT: DO NOT TOUCH ConnectionStringId here
            // (prevents accidental global ↔ connection corruption)

            await _context.SaveChangesAsync();
        }

        // =============================
        // ✅ GET GLOBAL FUNCTIONS
        // =============================
        public async Task<List<PromptFunction>> GetGlobalFunctionsAsync()
        {
            return await _context.PromptFunctions
                .Where(x => x.ConnectionStringId == null && !x.IsDeleted)
                .AsNoTracking() // 🔥 PERFORMANCE
                .ToListAsync();
        }
    }
}