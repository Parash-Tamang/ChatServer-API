using MediatR;

namespace AIChatbot.Application.Auth.Commands;

public record SendRegisterOtpCommand(string Email)
    : IRequest<bool>;