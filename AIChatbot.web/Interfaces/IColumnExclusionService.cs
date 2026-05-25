using AIChatbot.web.Dto;

namespace AIChatbot.web.Interfaces
{
    public interface IColumnExclusionService
    {
        Task<List<SchemaItemDto>> GetSchemaAsync(Guid connectionId);
        Task<List<string>> GetExclusionsAsync(Guid connectionId);
        Task<bool> SaveExclusionsAsync(ExclusionSaveDto dto);
    }
}