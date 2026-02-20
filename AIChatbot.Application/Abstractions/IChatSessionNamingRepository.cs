using AIChatbot.Domain.Entities;

public interface IChatSessionNamingRepository
{
    Task CreateAsync(ChatSessionNaming naming);
}
