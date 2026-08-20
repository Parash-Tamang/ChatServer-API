using MediatR;
using AIChatbot.Application.Auth.Results;

namespace AIChatbot.Application.Auth.Commands;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Password,
    string Role,
    string Token
) : IRequest<AuthResult>;
