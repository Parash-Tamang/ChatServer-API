using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Application.Dto.UserManagement;
using Agent.Application.Validators.Common;
using FluentValidation;
namespace Agent.Application.Validators.Auth
{
    public sealed class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator()
        {
            AuthCommonValidationRules.AddEmailRule(RuleFor(x => x.Email));
            AuthCommonValidationRules.AddEmailRule(RuleFor(x => x.Password));
        }
    }
}
