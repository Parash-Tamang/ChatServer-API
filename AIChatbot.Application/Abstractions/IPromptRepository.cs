using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IPromptRepository
{
    Task<PromptSet?> GetCurrentPromptAsync(Guid dbId);
    Task<PromptSet?> GetOriginalPromptAsync(Guid dbId);
    Task<List<PromptFunction>> GetFunctionsAsync(Guid promptSetId);

    Task AddPromptSetAsync(PromptSet set);
    Task AddFunctionsAsync(List<PromptFunction> functions);
    Task ReplaceFunctionsAsync(Guid promptSetId, List<PromptFunction> functions);
}