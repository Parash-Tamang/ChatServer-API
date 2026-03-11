using FluentValidation;
using AIChatbot.web.Models.Auth;
using AIChatbot.Web.Models.Auth;
public class RegisterValidator : AbstractValidator<RegisterUser>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .Matches(@"^(\+91)?[6-9]\d{9}$")
            .WithMessage("Invalid Indian phone number");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain special character");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => role == "User")
            .WithMessage("Invalid role");
    }
}
