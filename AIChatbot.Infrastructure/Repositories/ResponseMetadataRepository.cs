using AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;
using AIChatbot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIChatbot.Infrastructure.Repositories
{
    public class ResponseMetadataRepository : IResponseMetadataRepository
    {
        private readonly AppDbContext _db;

        public ResponseMetadataRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(ResponseMetadata metadata)
        {
            _db.ResponseMetadata.Add(metadata);
            await _db.SaveChangesAsync();
        }

        public async Task<ResponseMetadata?> GetByMessageIdAsync(Guid messageId)
        {
            return await _db.ResponseMetadata
                .FirstOrDefaultAsync(r => r.MessageId == messageId);
        }
    }
}