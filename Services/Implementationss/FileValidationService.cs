using DocumentSearch.Models;
using DocumentSearch.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DocumentSearch.Services.Implementationss;

public sealed class FileValidationService(IOptions<FileUploadSettings> options) : IFileValidationService
{
    private readonly FileUploadSettings _settings = options.Value;

    public async Task<IReadOnlyCollection<string>> ValidateAsync(
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (file == null || file.Length == 0)
        {
            return ["File is required."];
        }

        if (file.Length > _settings.MaxFileSizeBytes)
        {
            errors.Add(
                $"File size cannot exceed {_settings.MaxFileSizeBytes / 1024 / 1024} MB.");
        }

        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName) ||
            fileName.Length > 255 ||
            !string.Equals(fileName, file.FileName, StringComparison.Ordinal))
        {
            errors.Add("The file name is invalid or unsafe.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedContentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => null
        };

        if (expectedContentType == null)
        {
            errors.Add("Only PDF, JPEG, and PNG files are allowed.");
            return errors;
        }

        if (!string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("The declared content type does not match the file extension.");
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = await stream.ReadAsync(header, cancellationToken);

        var signatureIsValid = extension switch
        {
            ".pdf" => bytesRead >= 5 &&
                      header[0] == 0x25 && header[1] == 0x50 &&
                      header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D,
            ".jpg" or ".jpeg" => bytesRead >= 3 &&
                                 header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 &&
                      header.SequenceEqual(new byte[]
                      {
                          0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
                      }),
            _ => false
        };

        if (!signatureIsValid)
        {
            errors.Add("The file content does not match the file extension.");
        }

        return errors;
    }
}
