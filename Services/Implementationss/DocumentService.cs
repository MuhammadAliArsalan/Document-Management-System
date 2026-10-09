using DocumentSearch.ViewModels;
using DocumentSearch.Models;
using DocumentSearch.Repository.Interfaces;
using DocumentSearch.Services.Interfaces;


namespace DocumentSearch.Services.Implementationss
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IPdfTextExtractor _pdfTextExtractor;
        private readonly IWebHostEnvironment _environment;
        private readonly IOcrService _ocrService;
        private readonly IPdfRenderer _pdfRenderer;

        public DocumentService(
            IDocumentRepository documentRepository,
            IWebHostEnvironment environment,
            IPdfTextExtractor pdfTextExtractor,
            IOcrService ocrService,
            IPdfRenderer pdfRenderer)
        {
            _documentRepository = documentRepository;
            _environment = environment;
            _pdfTextExtractor = pdfTextExtractor;
            _ocrService = ocrService;
            _pdfRenderer = pdfRenderer;
        }

        public async Task<List<Document>> GetAllAsync()
        {
            return await _documentRepository.GetAllAsync();
        }

        public async Task<Document?> GetByIdAsync(int id)
        {
            return await _documentRepository.GetByIdAsync(id);
        }

        public async Task<Document> UploadAsync(DocumentUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("File is required.");

            var documentsPath = Path.Combine(
                _environment.ContentRootPath,
                "Documents");

            Directory.CreateDirectory(documentsPath);

            var extension = Path.GetExtension(dto.File.FileName)
                .ToLowerInvariant();

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var physicalPath = Path.Combine(
                documentsPath,
                uniqueFileName);

            // Save file
            await using (var stream = new FileStream(
                physicalPath,
                FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // Extract text
            var extractedText = string.Empty;

            if (extension == ".pdf")
            {
                // First we try normal PDF text extraction
                extractedText = _pdfTextExtractor.ExtractText(physicalPath);

                // If PDF has little/no text, treat it as scanned PDF
                if (string.IsNullOrWhiteSpace(extractedText) || extractedText.Trim().Length < 20)
                {
                    var ocrDirectory = Path.Combine(_environment.ContentRootPath,"TempOCR",Guid.NewGuid().ToString());

                    Directory.CreateDirectory(ocrDirectory);

                    try
                    {
                        var pageImages = _pdfRenderer.RenderPages(
                            physicalPath,
                            ocrDirectory);

                        foreach (var imagePath in pageImages)
                        {
                            var pageText =_ocrService.ExtractTextFromImage(imagePath);

                            extractedText += pageText + Environment.NewLine;
                        }
                    }
                    finally
                    {
                        // Delete temporary OCR images
                        if (Directory.Exists(ocrDirectory))
                        {
                            Directory.Delete(
                                ocrDirectory,
                                recursive: true);
                        }
                    }
                }
            }

            // Image
            else if (
                extension == ".jpg" ||
                extension == ".jpeg" ||
                extension == ".png")
            {
                extractedText = _ocrService.ExtractTextFromImage(physicalPath);
            }

            var document = new Document
            {
                UploadedByEmployeeId = dto.UploadedByEmployeeId,
                FileName = dto.File.FileName,
                ContentType = dto.File.ContentType,
                FilePath = Path.Combine("Documents", uniqueFileName),
                FileSize = dto.File.Length,
                UploadedAt = DateTime.UtcNow,
                ExtractedText = extractedText
            };

            return await _documentRepository.AddAsync(document);
        }


        public async Task<bool> UpdateAsync(
            int id,
            DocumentUpdateDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                throw new ArgumentException("File is required.");

            var existingDocument =
                await _documentRepository.GetByIdAsync(id);

            if (existingDocument == null)
                return false;

            // Delete old physical file
            var oldPhysicalPath = Path.Combine(
                _environment.ContentRootPath,
                existingDocument.FilePath);

            if (File.Exists(oldPhysicalPath))
            {
                File.Delete(oldPhysicalPath);
            }

            // Save new file
            var documentsPath = Path.Combine(
                _environment.ContentRootPath,
                "Documents");

            Directory.CreateDirectory(documentsPath);

            var extension = Path.GetExtension(dto.File.FileName)
                .ToLowerInvariant();

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var newPhysicalPath = Path.Combine(documentsPath, uniqueFileName);

            await using (var stream = new FileStream(
                newPhysicalPath,
                FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            // Extract text
            var extractedText = string.Empty;

            // PDF
            if (extension == ".pdf")
            {
                extractedText =_pdfTextExtractor.ExtractText(newPhysicalPath);

                // Scanned PDF fallback
                if (string.IsNullOrWhiteSpace(extractedText) || extractedText.Trim().Length < 20)
                {
                    var ocrDirectory = Path.Combine(
                        _environment.ContentRootPath,
                        "TempOCR",
                        Guid.NewGuid().ToString());

                    Directory.CreateDirectory(ocrDirectory);

                    try
                    {
                        var pageImages =
                            _pdfRenderer.RenderPages(
                                newPhysicalPath,
                                ocrDirectory);

                        foreach (var imagePath in pageImages)
                        {
                            var pageText = _ocrService.ExtractTextFromImage(imagePath);

                            extractedText += pageText + Environment.NewLine;
                        }
                    }
                    finally
                    {
                        if (Directory.Exists(ocrDirectory))
                        {
                            Directory.Delete(
                                ocrDirectory,
                                recursive: true);
                        }
                    }
                }
            }

            // Image
            else if (
                extension == ".jpg" ||
                extension == ".jpeg" ||
                extension == ".png")
            {
                extractedText = _ocrService.ExtractTextFromImage(newPhysicalPath);
            }

            // Update DB metadata
            existingDocument.FileName = dto.File.FileName;
            existingDocument.ContentType = dto.File.ContentType;
            existingDocument.FileSize = dto.File.Length;
            existingDocument.FilePath =
                Path.Combine("Documents", uniqueFileName);
            existingDocument.UploadedByEmployeeId =
                dto.UploadedByEmployeeId;
            existingDocument.ExtractedText =
                extractedText;

            await _documentRepository.UpdateAsync(
                existingDocument);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var document =
                await _documentRepository.GetByIdAsync(id);

            if (document == null)
                return false;

            // Delete physical file
            var physicalPath = Path.Combine(_environment.ContentRootPath,document.FilePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            await _documentRepository.DeleteAsync(document);

            return true;
        }

        public async Task<List<Document>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Search query is required.");

            query = query.Trim();

            return await _documentRepository.SearchAsync(query);
        }

    }
}
