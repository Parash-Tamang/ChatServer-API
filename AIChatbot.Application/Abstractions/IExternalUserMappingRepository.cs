using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IExternalUserMappingRepository
{
    Task AddAsync(
        ExternalUserMapping entity);

    Task<ExternalUserMapping?> GetAsync(
        string applicationUserId,
        Guid connectionId);
}