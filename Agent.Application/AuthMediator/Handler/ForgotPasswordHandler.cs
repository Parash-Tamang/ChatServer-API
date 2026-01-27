using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Agent.Application.AuthMediator.Command;
using Agent.Domain.Entities.UserManagement;
using MediatR;
using Agent.Application.Helpers;
namespace Agent.Application.Auth.Handler
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ApiResult<object>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ForgotPasswordHandler(UserManager<ApplicationUser> userManager) 
        {
            _userManager = userManager;
        }
        public async Task<ApiResult<object>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.userForgotPasswordDto.Email);
        
            if (user == null)
            {
                return ApiResult<object>.Fail("If the email exists, a password reset link has been sent.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = Uri.EscapeDataString(token);

            //var resetLink =
            //    $"https://localhost:7018/Account/Register/reset-password?email={user.Email}&token={encodedToken}";

            // TODO: Send resetLink via email here


            return ApiResult<object>.Ok(encodedToken, "If the email exists, a password reset link has been sent.");

        }
    }
}
