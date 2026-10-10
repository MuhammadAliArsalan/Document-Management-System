using DocumentSearch.Models;

namespace DocumentSearch.Repository.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetAllAsync(int employeeId);
        Task<Document?> GetByIdAsync(int id, int employeeId);
        Task<Document> AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task DeleteAsync(Document document);
        Task<List<Document>> SearchAsync(string query, int employeeId);
    }
}