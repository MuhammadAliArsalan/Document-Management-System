using Microsoft.AspNetCore.Http;

namespace DocumentSearch.Services.Interfaces;

public interface IFileValidationService
{
    Task<IReadOnlyCollection<string>> ValidateAsync(
        IFormFile? file,
        CancellationToken cancellationToken = default);
}
