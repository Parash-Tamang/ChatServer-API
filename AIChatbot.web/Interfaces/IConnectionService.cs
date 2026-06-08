using AIChatbot.web.Dto;
using AIChatbot.web.Models.Admin;
using AIChatbot.web.Models.Chat;

namespace AIChatbot.web.Interfaces
{
    public interface IConnectionService
    {
      
        Task<ServiceResult> SaveConnectionAsync(ConnectionRequestDto dto);
      
        Task<List<ConnectionRequestDto>> GetAllConnectionsAsync();
        Task<bool> UpdateConnectionAsync(ConnectionRequestDto dto);

        Task<bool> SetActiveConnectionAsync(Guid id, List<ConnectionRequestDto> allConnections);
        Task<ManagePromptsViewModel> GetManagePromptsAsync();
        Task<bool> ActivateConnectionAsync(Guid connectionId);
        Task<bool> CreateKnowledgeBaseAsync(Guid connectionId);
        Task<bool> UpdateKnowledgeBaseAsync(Guid connectionId);
        Task<bool> DeleteConnectionAsync(Guid connectionId);
        Task<bool> DeleteFunctionAsync(Guid connectionId, string functionId);


        Task<bool> SetPromptingModeAsync(string connectionId, int promptingMode);
    }
}