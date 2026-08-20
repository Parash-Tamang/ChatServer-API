using MediatR;

public record GetRolesQuery(string Token)
    : IRequest<List<string>>;