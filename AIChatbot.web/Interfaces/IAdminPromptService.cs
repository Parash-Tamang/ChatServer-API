using AIChatbot.web.Models.Admin;

namespace AIChatbot.web.Interfaces
{
    public interface IAdminPromptService
    {
        Task<List<GlobalPromptDto>> GetGlobalFunctionsAsync();
        Task<GlobalPromptDto?> GetFunctionByIdAsync(string functionId);
        Task<bool> SaveGlobalPromptAsync(SaveGlobalPromptRequest request);
        Task<List<GlobalPromptDto>> GetConnectionFunctionsAsync(string connectionId);
        Task<bool> SaveLocalPromptAsync(SaveLocalPromptRequest request);
    }
}