using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Validators.Common;
namespace Agent.Application.Validators.Auth
{
    public sealed class UserResetPasswordDtoValidator : AbstractValidator<UserResetPasswordDto>
    {
        public UserResetPasswordDtoValidator() 
        {
            AuthCommonValidationRules.AddEmailRule(RuleFor(x => x.Email));
            AuthCommonValidationRules.AddPasswordRule(RuleFor(x=>x.Password));
        }
    }
}
