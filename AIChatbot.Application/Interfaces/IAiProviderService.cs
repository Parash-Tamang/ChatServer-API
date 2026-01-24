using AIChatbot.Application.DTOs;

namespace AIChatbot.Application.Interfaces;

public interface IAiProviderService
{
    Task<string> GetReplyAsync(IEnumerable<MessageDto> context);
}
