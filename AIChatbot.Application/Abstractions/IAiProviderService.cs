using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;


/// Abstraction over external AI provider (e.g. OpenAI, Azure OpenAI, etc.)
/// Responsible only for generating assistant replies

public interface IAiProviderService
{
   
    /// Generates an AI reply based on conversation context
  
    Task<string> GetReplyAsync(IEnumerable<Message> context);
}
