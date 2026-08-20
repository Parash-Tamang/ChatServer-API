using MediatR;

namespace AIChatbot.Application.Chat.Commands;

public sealed record GenerateExcelCommand(
    Guid MessageId)
    : IRequest<byte[]>;