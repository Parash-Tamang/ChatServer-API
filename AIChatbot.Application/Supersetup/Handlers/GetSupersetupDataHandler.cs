using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using MediatR;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class GetSupersetupDataHandler
        : IRequestHandler<GetSupersetupDataQuery, SupersetupOverviewDto>
    {
        private readonly IConnectionRepository _connectionRepo;
        private readonly IPromptFunctionRepository _functionRepo;

        public GetSupersetupDataHandler(
            IConnectionRepository connectionRepo,
            IPromptFunctionRepository functionRepo)
        {
            _connectionRepo = connectionRepo;
            _functionRepo = functionRepo;
        }

        public async Task<SupersetupOverviewDto> Handle(
            GetSupersetupDataQuery request,
            CancellationToken ct)
        {
            var connections = await _connectionRepo.GetAllAsync();

            var result = new SupersetupOverviewDto();

            foreach (var conn in connections.Where(x => !x.IsDeleted))
            {
                result.Connections.Add(new ConnectionDto
                {
                    Id = conn.Id,
                    ServerName = conn.ServerName,
                    DatabaseName = conn.DatabaseName,
                    AuthMode = conn.AuthMode,
                    IsActive = conn.IsActive,
                    Verified = conn.Verified
                });

                var functions = await _functionRepo.GetByConnectionIdAsync(conn.Id);

                result.Functions[conn.Id] = functions.Select(f => new FunctionDto
                {
                    Id = f.Id,
                    FunctionName = f.FunctionName,
                    SystemPrompt = f.SystemPrompt
                }).ToList();
            }

            // GLOBAL FUNCTIONS
            var globalFunctions = await _functionRepo.GetGlobalFunctionsAsync();

            result.GlobalPrompt = globalFunctions.Select(f => new FunctionDto
            {
                Id = f.Id,
                FunctionName = f.FunctionName,
                SystemPrompt = f.SystemPrompt
            }).ToList();

            return result;
        }
    }
}