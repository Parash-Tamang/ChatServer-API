using FluentValidation;
using AIChatbot.Application.Auth.Commands;

namespace AIChatbot.Application.Auth.Validators;

public class RegisterUserValidator
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
        .Matches(@"^(\+91)?[6-9]\d{9}$")
        .WithMessage("Invalid Indian phone number");

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain uppercase")
            .Matches("[a-z]").WithMessage("Password must contain lowercase")
            .Matches("[0-9]").WithMessage("Password must contain digit");
    }
}
