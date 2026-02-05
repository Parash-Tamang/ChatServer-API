using AIChatbot.Application.Auth.Commands;
using FluentValidation;

namespace AIChatbot.Application.Auth.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
