using DocumentSearch.ViewModels;
using DocumentSearch.Models;

namespace DocumentSearch.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>> GetAllAsync();

        Task<Document?> GetByIdAsync(int id);

        Task<Document> UploadAsync(DocumentUploadDto dto);

        Task<bool> UpdateAsync(int id, DocumentUpdateDto dto);

        Task<bool> DeleteAsync(int id);

        Task<List<Document>> SearchAsync(string query);
    }
}
