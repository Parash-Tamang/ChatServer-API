using Agent.Application.Dto.UserManagement;
using MediatR;
using Agent.Application.Helpers;

namespace Agent.Application.AuthMediator.Command
{
    public record LoginCommand(UserLoginDto userLoginDto) : IRequest<ApiResult<AuthTokenResponseDto>>;
}
