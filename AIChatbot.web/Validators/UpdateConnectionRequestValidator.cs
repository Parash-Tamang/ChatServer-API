using AIChatbot.web.Models.Admin;
using FluentValidation;
using AIChatbot.web.Dto;


namespace AIChatbot.web.Validators
{
    public class UpdateConnectionRequestValidator : AbstractValidator<UpdateConnectionRequest>
    {
        public UpdateConnectionRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Connection ID is required.");

            RuleFor(x => x.ServerName)
                .NotEmpty().WithMessage("Server name is required.")
                .MaximumLength(200).WithMessage("Server name must not exceed 200 characters.");

            RuleFor(x => x.DatabaseName)
                .NotEmpty().WithMessage("Database name is required.")
                .MaximumLength(200).WithMessage("Database name must not exceed 200 characters.");

            RuleFor(x => x.AuthMode)
                .NotEmpty().WithMessage("Authentication mode is required.")
                .Must(x => x == "Sql" || x == "Windows")
                .WithMessage("Auth mode must be Sql or Windows.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required for SQL Authentication.")
                .When(x => x.AuthMode == "Sql");

            RuleFor(x => x.ConnectionTimeout)
                .InclusiveBetween(0, 300).WithMessage("Timeout must be between 0 and 300 seconds.");
        }
    }
}