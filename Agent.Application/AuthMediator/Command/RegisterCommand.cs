using MediatR;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
namespace Agent.Application.AuthMediator.Command
{
    public record RegisterCommand(UserRegisterDto userRegisterDto) : IRequest<ApiResult<AuthTokenResponseDto>>;
    //{
    //    public string FirstName { get; set; }

    //    public string LastName { get; set; }
    //    public string Email { get; set; }
    //    public string Phone { get; set; }
    //    public string Password { get; set; }
    //}

}


