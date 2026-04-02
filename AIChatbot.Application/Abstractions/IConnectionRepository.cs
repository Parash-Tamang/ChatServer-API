using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbot.Application.Abstractions;
using AIChatbot.Domain.Entities;

public interface IConnectionRepository
{
    Task<List<ConnectionString>> GetAllAsync();
    Task<ConnectionString?> GetByIdAsync(Guid id);
    Task<ConnectionString?> GetByUniqueKeyAsync(
    string serverName,
    string databaseName,
    string authMode);

    Task AddAsync(ConnectionString entity);
    Task UpdateAsync(ConnectionString entity);
    Task SetActiveAsync(Guid id);
}