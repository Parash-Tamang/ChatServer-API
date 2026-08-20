using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.DTOs;
using AIChatbot.Application.Supersetup.Queries;
using MediatR;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class GetFunctionByIdHandler
        : IRequestHandler<GetFunctionByIdQuery, FunctionDto>
    {
        private readonly IPromptFunctionRepository _globalRepo;
        private readonly ILocalFunctionRepository _localRepo;

        public GetFunctionByIdHandler(
            IPromptFunctionRepository globalRepo,
            ILocalFunctionRepository localRepo)
        {
            _globalRepo = globalRepo;
            _localRepo = localRepo;
        }

        public async Task<FunctionDto> Handle(
            GetFunctionByIdQuery request,
            CancellationToken ct)
        {
            // =========================================
            // 🔹 TRY GLOBAL FUNCTION
            // =========================================

            var global = await _globalRepo
                .GetByIdAsync(request.FunctionId);

            if (global != null)
            {
                return new FunctionDto
                {
                    Id = global.Id,
                    FunctionName = global.FunctionName,
                    SystemPrompt = global.SystemPrompt,
                    Source = "global"
                };
            }

            // =========================================
            // 🔹 TRY LOCAL OVERRIDE
            // =========================================

            var local = await _localRepo
                .GetByIdAsync(request.FunctionId);

            if (local != null)
            {
                return new FunctionDto
                {
                    Id = local.Id,
                    FunctionName = local.FunctionName,
                    SystemPrompt = local.OverridePrompt ?? string.Empty,
                    Source = "local"
                };
            }

            // =========================================
            // ❌ NOT FOUND
            // =========================================

            throw new KeyNotFoundException("Function not found.");
        }
    }
}