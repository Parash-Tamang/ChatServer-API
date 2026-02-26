using AIChatbot.Domain.Entities;

namespace AIChatbot.Application.Abstractions;

public interface IConnectionRepository
{
    Task<ConnectionString?> GetByIdAsync(Guid id);
    Task<List<ConnectionString>> GetAllAsync();
    Task AddAsync(ConnectionString entity);
    Task UpdateAsync(ConnectionString entity);
    Task DeleteAsync(ConnectionString entity);
}