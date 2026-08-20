using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions
{
    public interface IPromptFunctionRepository
    {
        Task AddAsync(PromptFunction entity);
        Task UpdateAsync(PromptFunction entity);
        Task<PromptFunction?> GetByIdAsync(Guid id);

        Task<List<PromptFunction>> GetByConnectionIdAsync(Guid connectionId);
        Task<List<PromptFunction>> GetGlobalFunctionsAsync();
        Task<PromptFunction?> GetByNameAsync(string functionName);
    }
}