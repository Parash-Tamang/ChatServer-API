using FluentValidation;
using AIChatbot.Application.Chat.Commands;

namespace AIChatbot.Application.Chat.Validators;


/// Validates chat message input

public class SendChatMessageValidator
    : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
