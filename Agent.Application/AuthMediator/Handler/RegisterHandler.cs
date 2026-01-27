using Agent.Application.AuthMediator.Command;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Helpers;
using Agent.Application.Interface;
using Agent.Domain.Entities.UserManagement;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Auth.Handler
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, ApiResult<AuthTokenResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthService _authService;

        public RegisterHandler(UserManager<ApplicationUser> userManager, IAuthService authService)
        {
            _userManager = userManager;
            _authService = authService;
        }

        public async Task<ApiResult<AuthTokenResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.userRegisterDto;
            var emailUser = await _userManager.FindByEmailAsync(dto.Email);

            if (emailUser != null)
            {
                string message = "Email already registered.";
                return ApiResult<AuthTokenResponseDto>.Fail(message);
            }
            var user = new ApplicationUser
            {
                UserName = request.userRegisterDto.Email,
                FirstName = request.userRegisterDto.FirstName,
                LastName = request.userRegisterDto.LastName,
                Email = request.userRegisterDto.Email,
                PhoneNumber = request.userRegisterDto.Phone,
                FullName = request.userRegisterDto.FirstName + " " + request.userRegisterDto.LastName
            };

            var result = await _userManager.CreateAsync(user, request.userRegisterDto.Password);

            if (!result.Succeeded)
            {
                //var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                //throw new Exception(errors

                return ApiResult<AuthTokenResponseDto>.Fail("User Registration Failed.");
            }

            await _userManager.AddToRoleAsync(user, "User");

            var data = await _authService.AuthTokenResponseAsync(user);

            return ApiResult<AuthTokenResponseDto>.Ok(data,"User Registered Successfully");

        }
    }
}
