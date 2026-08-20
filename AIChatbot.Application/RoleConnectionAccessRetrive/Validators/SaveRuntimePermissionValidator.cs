using AIChatbot.Application.RoleConnectionAccessRetrive.Commands;
using FluentValidation;

namespace AIChatbot.Application.RoleConnectionAccessRetrive.Validators;

public class SaveRuntimePermissionValidator
    : AbstractValidator<SaveRuntimePermissionCommand>
{
    public SaveRuntimePermissionValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty();

        RuleFor(x => x.SelectedRoleId)
            .NotEmpty();

        RuleFor(x => x.Database)
            .NotEmpty();

        RuleFor(x => x.ConnectionId)
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .NotNull();
    }
}