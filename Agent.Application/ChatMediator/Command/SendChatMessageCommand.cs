using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Validators.Chats;
using Agent.Application.Dto.UserManagement;
using MediatR;
using Agent.Application.Helpers;

namespace Agent.Application.ChatMediator.Command
{
    public record SendChatMessageCommand(ChatRequestDto chatRequestDto,string userId) : IRequest<ApiResult<ChatResponseDto>>;
}
