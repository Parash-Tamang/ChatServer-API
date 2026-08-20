using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Supersetup.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIChatbot.Application.Supersetup.Queries;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class GetConnectionsHandler
        : IRequestHandler<GetConnectionsQuery, List<ConnectionDto>>
    {
        private readonly IConnectionRepository _repo;

        public GetConnectionsHandler(IConnectionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ConnectionDto>> Handle(
            GetConnectionsQuery request,
            CancellationToken ct)
        {
            var connections = await _repo.GetAllAsync();

            return connections
                .Where(x => !x.IsDeleted)
                .Select(c => new ConnectionDto
                {
                    Id = c.Id,
                    ServerName = c.ServerName,
                    DatabaseName = c.DatabaseName,
                    AuthMode = c.AuthMode,
                    IsActive = c.IsActive,
                    Verified = c.Verified,
                    ConnectionTimeout = c.ConnectionTimeout,
                    CreatedAt = c.CreatedAt,
                    TrustCertificate = c.TrustCertificate,
                   
                })
                .ToList();
        }
    }
}
