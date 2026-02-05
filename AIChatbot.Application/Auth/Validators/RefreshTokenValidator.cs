using AIChatbot.Application.Auth.Commands;
using FluentValidation;

namespace AIChatbot.Application.Auth.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}
