using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IConnectionExclusionRepository
{
    Task SaveAsync(List<ConnectionExclusion> entities);

    Task<List<ConnectionExclusion>> GetByConnectionAsync(
        Guid connectionId);

    Task DeleteByConnectionAsync(Guid connectionId);
}