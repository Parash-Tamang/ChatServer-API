using Agent.Application.Dto.UserManagement;
using Agent.Domain.Entities.UserManagement;
using System;

namespace Agent.Application.Interface
{
    public interface IChatMessageService
    {
        Task<UserMessage> StoreChatMessageAsync(UserMessageRequestDto userMessageRequestDto);
    }
}

