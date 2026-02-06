using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
namespace Agent.Application.Interface
{
    public interface IAgentService
    {
        Task<UserMessageResponseDto> SendDataAgentAsync (UserMessageRequestDto userMessageRequestDto);
        Task<SQLQueryResponseDto> ExecuteSQlQueryAsync(SQLQueryReceivedDto sqlQueryDto);

        Task<AgentResponseDto?> GenerateAsync(string request, List<UserMessageResponseDto> lastMessages);

    }
}

