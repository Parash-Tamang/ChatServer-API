using AIChatbot.web.Dto;
using AIChatbot.web.Models.Admin;
using AIChatbot.web.Models.Chat;

namespace AIChatbot.web.Interfaces
{
    public interface IConnectionService
    {
        //Task<bool> ConnectAsync(SqlConnectionViewModel model);
        Task<ServiceResult> SaveConnectionAsync(ConnectionRequestDto dto);
      
        Task<List<ConnectionRequestDto>> GetAllConnectionsAsync();
        Task<bool> UpdateConnectionAsync(ConnectionRequestDto dto);
        Task<bool> SetActiveConnectionAsync(Guid id, List<ConnectionRequestDto> allConnections);
        Task<ManagePromptsViewModel> GetManagePromptsAsync();
        Task<bool> ActivateConnectionAsync(Guid connectionId);
        Task<bool> UpdateKnowledgeBaseAsync(Guid connectionId);
        Task<bool> DeleteConnectionAsync(Guid connectionId);
        Task<bool> DeleteFunctionAsync(Guid connectionId, string functionId);

        // afer these it is for the prompts
        //Task<GlobalPromptDto> GetGlobalPromptAsync();
        //Task<bool> SaveGlobalPromptAsync(GlobalPromptDto dto);
        //Task<LocalPromptDto> GetLocalPromptAsync(string functionId);
        //Task<LocalPromptDto> GetConnectionFunctionAsync(string connectionId);
        //Task<bool> SaveLocalPromptAsync(LocalPromptDto dto);
        Task<bool> SetPromptingModeAsync(string connectionId, int promptingMode);
    }
}