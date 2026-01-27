using Agent.Application.Validators.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Dto.UserManagement;
namespace Agent.Application.Validators.Auth
{
    public class UserForgotPasswordDtoValidator : AbstractValidator<UserForgotPasswordDto>
    {
        public UserForgotPasswordDtoValidator() 
        {
            AuthCommonValidationRules.AddEmailRule(RuleFor(x => x.Email));
        }
    }
}
