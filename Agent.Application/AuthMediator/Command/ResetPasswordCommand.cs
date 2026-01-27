using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.AuthMediator.Command
{
    public record ResetPasswordCommand(UserResetPasswordDto userResetPasswordDto) : IRequest<ApiResult<object>>;
}
