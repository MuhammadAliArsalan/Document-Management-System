using DocumentSearch.Models;
using DocumentSearch.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace DocumentSearch.Services.Implementationss
{
    public class FileValidationService : IFileValidationService
    {
        private readonly FileUploadSettings _settings;

        public FileValidationService(IOptions<FileUploadSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<IReadOnlyCollection<string>> ValidateAsync(
            IFormFile? file,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            List<string> errors = new List<string>();

            if (file == null || file.Length == 0)
            {
                errors.Add("File is required.");
                return errors;
            }

            if (file.Length > _settings.MaxFileSizeBytes)
            {
                errors.Add("File size cannot exceed " + (_settings.MaxFileSizeBytes / 1024 / 1024) + " MB.");
            }

            string fileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName.Length > 255 ||
                !string.Equals(fileName, file.FileName, StringComparison.Ordinal))
            {
                errors.Add("The file name is invalid or unsafe.");
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            string? expectedContentType = null;

            switch (extension)
            {
                case ".pdf":
                    expectedContentType = "application/pdf";
                    break;

                case ".jpg":
                case ".jpeg":
                    expectedContentType = "image/jpeg";
                    break;

                case ".png":
                    expectedContentType = "image/png";
                    break;

                default:
                    expectedContentType = null;
                    break;
            }

            if (expectedContentType == null)
            {
                errors.Add("Only PDF, JPEG, and PNG files are allowed.");
                return errors;
            }

            if (!string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("The declared content type does not match the file extension.");
            }

            byte[] header = new byte[8];
            int bytesRead = 0;

            using (Stream stream = file.OpenReadStream())
            {
                bytesRead = await stream.ReadAsync(header, 0, header.Length, cancellationToken);
            }

            bool signatureIsValid = false;

            switch (extension)
            {
                case ".pdf":
                    // %PDF-
                    signatureIsValid = bytesRead >= 5 &&
                                       header[0] == 0x25 && header[1] == 0x50 &&
                                       header[2] == 0x44 && header[3] == 0x46 &&
                                       header[4] == 0x2D;
                    break;

                case ".jpg":
                case ".jpeg":
                    signatureIsValid = bytesRead >= 3 &&
                                       header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                    break;

                case ".png":
                    byte[] pngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                    signatureIsValid = bytesRead >= 8 && header.SequenceEqual(pngSignature);
                    break;

                default:
                    signatureIsValid = false;
                    break;
            }

            if (!signatureIsValid)
            {
                errors.Add("The file content does not match the file extension.");
            }

            return errors;
        }
    }
}