using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIChatbot.Application.Common;
using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class ActivateConnectionHandler
        : IRequestHandler<ActivateConnectionCommand, GenericResult>
    {
        private readonly IConnectionRepository _repo;

        public ActivateConnectionHandler(IConnectionRepository repo)
        {
            _repo = repo;
        }

        public async Task<GenericResult> Handle(
            ActivateConnectionCommand request,
            CancellationToken ct)
        {
            var conn = await _repo.GetByIdAsync(request.ConnectionId)
                ?? throw new KeyNotFoundException("Connection not found.");

            if (!conn.Verified)
                throw new BadHttpRequestException("Connection must be verified before activation.");

            await _repo.SetActiveAsync(conn.Id);

            return new GenericResult
            {
                Success = true,
                Message = "Knowledgebase activated successfully."
            };

        }
    }
}
