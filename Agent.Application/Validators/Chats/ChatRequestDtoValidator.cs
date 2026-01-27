using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Agent.Application.Dto.UserManagement;
namespace Agent.Application.Validators.Chats
{
    public class ChatRequestDtoValidator : AbstractValidator<ChatRequestDto>
    {
        public ChatRequestDtoValidator() {
            RuleFor(x => x.sessionId)
                .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _))
                .WithMessage("SessionId must be a valid GUID if provided");

            RuleFor(x => x.message)
                .NotEmpty()
                .WithMessage("User Message is empty");
        }
    }
}
