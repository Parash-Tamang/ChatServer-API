using Agent.Application.Dto.UserManagement;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using Agent.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Infrastructure.Services
{
    public class ChatMessageService : IChatMessageService
    {
        private readonly ApplicationDbContext _DbContext;

        public ChatMessageService(ApplicationDbContext DbContext)
        {
            _DbContext = DbContext;
        }
        
        public async Task<UserMessage> StoreChatMessageAsync(UserMessageRequestDto userMessageRequestDto)
        {
            UserMessage message = new UserMessage 
            { 
                SessionId = userMessageRequestDto.SessionId, 
                MessageText = userMessageRequestDto.MessageText,
                SenderType = userMessageRequestDto.SenderType,
            };

            await _DbContext.UserMessages.AddAsync(message);
            await _DbContext.SaveChangesAsync();

            return message;
           
        }
    }
}
