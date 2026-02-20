using FluentValidation;
using AIChatbot.Application.Auth.Commands;

namespace AIChatbot.Application.Auth.Validators;

public class LoginValidator
    : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}
