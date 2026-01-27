using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using MediatR;
namespace Agent.Application.AuthMediator.Command
{
    public record LogoutCommand(string refreshTokenRequestDto ) : IRequest<ApiResult<object>>;
}
