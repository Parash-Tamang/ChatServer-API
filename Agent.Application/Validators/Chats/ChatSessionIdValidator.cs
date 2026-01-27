using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Application.Validators.Chats
{
    using FluentValidation;

    public class ChatSessionIdValidator : AbstractValidator<string>
    {
        public ChatSessionIdValidator()
        {
            RuleFor(x => x)
                .NotEmpty()
                .WithMessage("SessionId is required")
                .Must(id => Guid.TryParse(id, out _))
                .WithMessage("SessionId must be a valid GUID");
        }
    }
}
