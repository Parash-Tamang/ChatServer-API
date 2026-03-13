using FluentValidation;
using AIChatbot.web.Models.Auth;
namespace AIChatbot.web.Validators

{
    public class LoginValidator : AbstractValidator<LoginUser>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Invalid email format");
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required");
        }   
    }
}
