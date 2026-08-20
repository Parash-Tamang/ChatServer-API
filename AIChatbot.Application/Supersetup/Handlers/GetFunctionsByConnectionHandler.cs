using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Application.Supersetup.Queries;
using MediatR;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class GetFunctionsByConnectionHandler
        : IRequestHandler<GetFunctionsByConnectionQuery, List<FunctionDto>>
    {
        private readonly ILocalFunctionRepository _localRepo;
        private readonly IPromptFunctionRepository _globalRepo;

        public GetFunctionsByConnectionHandler(
            ILocalFunctionRepository localRepo,
            IPromptFunctionRepository globalRepo)
        {
            _localRepo = localRepo;
            _globalRepo = globalRepo;
        }

        public async Task<List<FunctionDto>> Handle(
            GetFunctionsByConnectionQuery request,
            CancellationToken ct)
        {
            // 🔹 ALL GLOBAL FUNCTIONS
            var globals = await _globalRepo.GetGlobalFunctionsAsync();

            // 🔹 LOCAL OVERRIDES
            var localBlocks = await _localRepo
                .GetByConnectionAsync(request.ConnectionId);

            // 🔥 GLOBAL-FIRST ARCHITECTURE
            var result = globals
                .Where(g => !g.IsDeleted)
                .Select(global =>
                {
                    var local = localBlocks.FirstOrDefault(
                        x => x.FunctionName == global.FunctionName);

                    return new FunctionDto
                    {
                        Id = local?.Id ?? global.Id,

                        FunctionName = global.FunctionName,

                        SystemPrompt =
                            local != null && local.IsOverride
                                ? local.OverridePrompt!
                                : global.SystemPrompt,

                        Source =
                            local != null && local.IsOverride
                                ? "local"
                                : "global"
                    };
                })
                .ToList();

            return result;
        }
    }
}