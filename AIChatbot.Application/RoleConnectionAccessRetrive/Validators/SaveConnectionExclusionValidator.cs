using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using FluentValidation;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Validators;

public class SaveConnectionExclusionValidator
    : AbstractValidator<SaveConnectionExclusionCommand>
{
    public SaveConnectionExclusionValidator()
    {
        RuleFor(x => x.ConnectionId)
            .NotEmpty();

        RuleForEach(x => x.Exclusions)
            .ChildRules(schema =>
            {
                schema.RuleFor(x => x.SchemaName)
                    .NotEmpty();
            });
    }
}