using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
namespace Agent.Application.Validators.Common
{
    public class AuthCommonValidationRules
    {
        public static void AddEmailRule<T>(IRuleBuilderInitial<T,string> rule)
        {
            rule.NotEmpty().WithMessage("Email is required").
               EmailAddress().WithMessage("Invalid email format.");
        }
        public static void AddPasswordRule<T>(IRuleBuilderInitial<T, string> rule)
        {
            rule.NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
        }



    }
}
