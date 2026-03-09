using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;

public interface IPromptFunctionRepository
{
    Task<List<PromptFunction>> GetByConnectionIdAsync(Guid connectionId);
    Task<PromptFunction?> GetByIdAsync(Guid id);
    Task AddAsync(PromptFunction entity);
    Task UpdateAsync(PromptFunction entity);
    Task<List<PromptFunction>> GetGlobalFunctionsAsync();
}