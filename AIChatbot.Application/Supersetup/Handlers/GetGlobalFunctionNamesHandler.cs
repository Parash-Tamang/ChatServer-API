using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Application.Supersetup.Queries;
using MediatR;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class GetGlobalFunctionNamesHandler
        : IRequestHandler<GetGlobalFunctionNamesQuery, List<FunctionDto>>
    {
        private readonly IPromptFunctionRepository _repo;

        public GetGlobalFunctionNamesHandler(IPromptFunctionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<FunctionDto>> Handle(
            GetGlobalFunctionNamesQuery request,
            CancellationToken ct)
        {
            var functions = await _repo.GetGlobalFunctionsAsync();

            return functions
                .Where(f => !f.IsDeleted)
                .Select(f => new FunctionDto
                {
                    Id = f.Id,
                    FunctionName = f.FunctionName
                })
                .ToList();
        }
    }
}