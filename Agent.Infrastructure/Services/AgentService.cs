using Agent.Application.Dto.UserManagement;
using Agent.Application.Interface;

namespace Agent.Infrastructure.Services
{
    public class AgentService : IAgentService 
    {
        public Task<UserMessageResponseDto> SendDataAgentAsync(UserMessageRequestDto userMessageRequestDto)
        {
            throw new NotImplementedException();

        }

        public Task<SQLQueryResponseDto> ExecuteSQlQueryAsync(SQLQueryReceivedDto sqlQueryDto)
        {
            throw new NotImplementedException();
        }

        public string GenerateAsync(string request)
        {
            return "Hi, how can i help you!";
        }

    }
}
