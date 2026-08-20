using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions
{
    using AIChatbot.Domain.Entities;

    public interface ILocalFunctionRepository
    {
        Task<LocalFunctionBlock?> GetAsync(Guid connectionId, string functionName);

        Task<List<LocalFunctionBlock>> GetByConnectionAsync(Guid connectionId);

        Task AddRangeAsync(List<LocalFunctionBlock> blocks);

        Task UpdateAsync(LocalFunctionBlock block);
        Task RemoveOverridesByFunctionName(string functionName);
        Task DeleteByConnectionIdAsync(Guid connectionId);
        Task<LocalFunctionBlock?> GetByIdAsync(Guid id);
    }
}
