using Agent.Application.Dto.UserManagement;
using FluentValidation;
using Agent.Application.Validators.Common;
namespace Agent.Application.Validators.Auth
{
    public sealed class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator() 
        {
            AuthCommonValidationRules.AddEmailRule(RuleFor(x => x.Email));

            AuthCommonValidationRules.AddPasswordRule(RuleFor(x => x.Password));
      
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Password does not match");

            RuleFor(x => x.Phone)
                .Matches(@"^\d{10}$")
                .WithMessage("Phone number must be exactly 10 digits")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));


            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required.").
                MinimumLength(3).WithMessage("First Name must be at least 3 characters long");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("First Name is required.").
                MinimumLength(3).WithMessage(":Last Name must be at least 3 characters long");

        }
    }
}
