using AIChatbot.web.Models.Chat;

namespace AIChatbot.web.Interfaces
{
    public interface IConnectionService
    {
        Task<bool> ConnectAsync(SqlConnectionViewModel model);
    }
}
