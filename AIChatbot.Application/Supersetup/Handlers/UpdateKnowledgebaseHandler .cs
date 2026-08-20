using AIChatbot.Application.Abstractions;
using AIChatbot.Application.Common;
using AIChatbot.Application.Supersetup.Commands;
using MediatR;

namespace AIChatbot.Application.Supersetup.Handlers
{
    public class UpdateKnowledgebaseHandler
        : IRequestHandler<UpdateKnowledgebaseCommand, GenericResult>
    {
        private readonly IConnectionRepository _repo;
        private readonly IAiProviderService _ai;

        public UpdateKnowledgebaseHandler(
            IConnectionRepository repo,
            IAiProviderService ai)
        {
            _repo = repo;
            _ai = ai;
        }

        public async Task<GenericResult> Handle(
            UpdateKnowledgebaseCommand request,
            CancellationToken ct)
        {
            var conn = await _repo.GetByIdAsync(request.ConnectionId)
                ?? throw new KeyNotFoundException("Connection not found.");

            var result = await _ai.UpdateDatabaseAsync(conn); // ✅ FIXED

            if (!result.DbStatus)
                throw new ApplicationException("Knowledgebase update failed.");

            return new GenericResult
            {
                Success = true,
                Message = "Knowledgebase updated successfully."
            };
        }
    }
}