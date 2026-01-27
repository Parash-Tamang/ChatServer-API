using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Helpers;
using Agent.Application.Dto.UserManagement;
using MediatR;
namespace Agent.Application.ChatMediator.Command
{
    public record SendAllMessageCommand(string sessionId) : IRequest<ApiResult<ChatHistoryResponseDto>>;
}
