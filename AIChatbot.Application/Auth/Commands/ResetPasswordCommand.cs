using MediatR;

public record ResetPasswordCommand(
    string ResetToken,
    string NewPassword
) : IRequest<Unit>;