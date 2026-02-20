using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IResponseMetadataRepository
{
    Task SaveAsync(ResponseMetadata metadata);
}
