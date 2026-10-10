using DocumentSearch.Helpers;
using DocumentSearch.ViewModels;

namespace DocumentSearch.Services.Interfaces;

public interface IDocumentService
{
    Task<ServiceResult<IReadOnlyList<DocumentResponse>>> GetAllAsync(int employeeId);
    Task<ServiceResult<DocumentResponse>> GetByIdAsync(int id, int employeeId);
    Task<ServiceResult<DocumentResponse>> UploadAsync(
        DocumentUploadDto dto,
        int employeeId,
        CancellationToken cancellationToken = default);
    Task<ServiceResult<DocumentResponse>> UpdateAsync(
        int id,
        DocumentUpdateDto dto,
        int employeeId,
        CancellationToken cancellationToken = default);
    Task<ServiceResult<object?>> DeleteAsync(int id, int employeeId);
    Task<ServiceResult<IReadOnlyList<DocumentResponse>>> SearchAsync(
        string query,
        int employeeId);
}
