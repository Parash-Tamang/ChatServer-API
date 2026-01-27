using Agent.Domain.Entities.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Dto.UserManagement;
namespace Agent.Application.Interface
{
    public interface IChatSessionService
    {
        Task<UserSession> CreateChatSessionAsync(string message, string userId); 
        Task<UserSession?>GetExistAsync (string sessionId);

        Task<List<UserMessageResponseDto>> GetChatMessageBySessionId(string sessionId);

        Task<SessionTitleResponseDto> GetUserSessionsAsync(string userId);

    }
}
