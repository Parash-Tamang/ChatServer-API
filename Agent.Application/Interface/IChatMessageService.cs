using Agent.Application.Dto.UserManagement;
using System;

namespace Agent.Application.Interface
{
    public interface IChatMessageService
    {
        Task StoreChatMessageAsync(UserMessageRequestDto userMessageRequestDto);
    }
}

