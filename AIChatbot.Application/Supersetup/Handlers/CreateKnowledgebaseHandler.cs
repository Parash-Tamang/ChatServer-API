using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class CreateKnowledgebaseHandler
     : IRequestHandler<CreateKnowledgebaseCommand, GenericResult>
    {
        private readonly IConnectionRepository _repo;
        private readonly IAiProviderService _ai;

        public CreateKnowledgebaseHandler(
            IConnectionRepository repo,
            IAiProviderService ai)
        {
            _repo = repo;
            _ai = ai;
        }
        public async Task<GenericResult> Handle(
            CreateKnowledgebaseCommand request,
            CancellationToken ct)
        {
            var conn =
                await _repo.GetByIdAsync(request.ConnectionId)
                ?? throw new KeyNotFoundException(
                    "Connection not found.");

            var result =
                await _ai.PrepareDatabaseAsync(conn);

            if (!result.DbStatus)
            {
                throw new ApplicationException(
                    "Knowledgebase creation failed.");
            }

            conn.Verified = true;
            conn.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(conn);

            return new GenericResult
            {
                Success = true,
                Message = "Knowledgebase created successfully."
            };
        }
    }
}