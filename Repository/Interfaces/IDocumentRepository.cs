using DocumentSearch.Models;

namespace DocumentSearch.Repository.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetAllAsync();

        Task<Document?> GetByIdAsync(int id);

        Task<Document> AddAsync(Document document);

        Task UpdateAsync(Document document);

        Task DeleteAsync(Document document);

        Task<List<Document>> SearchAsync(string query);
    }
}