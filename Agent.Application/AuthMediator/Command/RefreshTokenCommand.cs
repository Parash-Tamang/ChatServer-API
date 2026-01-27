using Agent.Application.Dto.UserManagement;
using MediatR;
using Agent.Application.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.AuthMediator.Command
{
    public record RefreshTokenCommand(RefreshTokenRequestDto refreshTokenRequestDto) : IRequest<ApiResult<AuthTokenResponseDto>>;
}
