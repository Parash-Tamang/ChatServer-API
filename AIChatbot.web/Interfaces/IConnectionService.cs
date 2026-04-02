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
    }
}