using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using AIChatbot.Application.Supersetup.DTOs;
using MediatR;
using System.Collections.Generic;
using AIChatbot.Domain.Entities;

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
            var connections = (await _connectionRepo.GetAllAsync())
                .Where(x => !x.IsDeleted)
                .ToList();

            var result = new SupersetupOverviewDto
            {
                Connections = new List<ConnectionDto>(),
                Functions = new Dictionary<Guid, List<FunctionDto>>(),
                GlobalPrompt = new List<FunctionDto>()
            };

            // ============================================================
            // 🔥 LOAD ALL FUNCTIONS ONCE (OPTIMIZATION)
            // ============================================================
            var allFunctions = new List<PromptFunction>();

            foreach (var conn in connections)
            {
                var funcs = await _functionRepo.GetByConnectionIdAsync(conn.Id);
                allFunctions.AddRange(funcs);
            }

            // ============================================================
            // 🔹 CONNECTIONS + FUNCTIONS
            // ============================================================
            foreach (var conn in connections)
            {
                result.Connections.Add(new ConnectionDto
                {
                    Id = conn.Id,
                    ServerName = conn.ServerName,
                    DatabaseName = conn.DatabaseName,
                    AuthMode = conn.AuthMode,
                    IsActive = conn.IsActive,
                    Verified = conn.Verified,
                    ConnectionTimeout = conn.ConnectionTimeout,
                    TrustCertificate = conn.TrustCertificate,
                    CreatedAt = conn.CreatedAt,


                    // ✅ ADDED (IMPORTANT)
                    PromptingMode = conn.PromptingMode
                });

                var functions = allFunctions
                    .Where(f => f.ConnectionStringId == conn.Id)
                    .ToList();

                result.Functions[conn.Id] = functions.Select(f => new FunctionDto
                {
                    Id = f.Id,
                    FunctionName = f.FunctionName,
                    SystemPrompt = f.SystemPrompt
                }).ToList();
            }

            // ============================================================
            // 🌍 GLOBAL FUNCTIONS
            // ============================================================
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