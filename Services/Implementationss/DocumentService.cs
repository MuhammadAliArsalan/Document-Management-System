using DocumentSearch.Helpers;
using DocumentSearch.Models;
using DocumentSearch.Repository.Interfaces;
using DocumentSearch.Services.Interfaces;
using DocumentSearch.ViewModels;

namespace DocumentSearch.Services.Implementationss;

public sealed class DocumentService : IDocumentService
{
    private const int ResponseTextLimit = 1000;
    private readonly IDocumentRepository _documentRepository;
    private readonly IPdfTextExtractor _pdfTextExtractor;
    private readonly IWebHostEnvironment _environment;
    private readonly IOcrService _ocrService;
    private readonly IPdfRenderer _pdfRenderer;
    private readonly IFileValidationService _fileValidationService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IWebHostEnvironment environment,
        IPdfTextExtractor pdfTextExtractor,
        IOcrService ocrService,
        IPdfRenderer pdfRenderer,
        IFileValidationService fileValidationService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _environment = environment;
        _pdfTextExtractor = pdfTextExtractor;
        _ocrService = ocrService;
        _pdfRenderer = pdfRenderer;
        _fileValidationService = fileValidationService;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<DocumentResponse>>> GetAllAsync(
        int employeeId)
    {
        var documents = await _documentRepository.GetAllAsync(employeeId);
        var response = documents.Select(ToResponse).ToList();
        return ServiceResult<IReadOnlyList<DocumentResponse>>.Success(
            response,
            "Documents retrieved successfully.");
    }

    public async Task<ServiceResult<DocumentResponse>> GetByIdAsync(
        int id,
        int employeeId)
    {
        var document = await _documentRepository.GetByIdAsync(id, employeeId);
        return document == null
            ? ServiceResult<DocumentResponse>.NotFound("Document not found.")
            : ServiceResult<DocumentResponse>.Success(
                ToResponse(document),
                "Document retrieved successfully.");
    }

    public async Task<ServiceResult<DocumentResponse>> UploadAsync(
        DocumentUploadDto dto,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        var errors = await _fileValidationService.ValidateAsync(dto.File, cancellationToken);
        if (errors.Count > 0)
        {
            return ServiceResult<DocumentResponse>.ValidationError(
                "File validation failed.",
                errors);
        }

        var file = dto.File!;
        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var documentsPath = Path.Combine(_environment.ContentRootPath, "Documents");
        Directory.CreateDirectory(documentsPath);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var physicalPath = Path.Combine(documentsPath, uniqueFileName);

        try
        {
            await SaveFileAsync(file, physicalPath, cancellationToken);
            var extractedText = ExtractText(physicalPath, extension);
            var document = new Document
            {
                UploadedByEmployeeId = employeeId,
                FileName = originalFileName,
                ContentType = file.ContentType,
                FilePath = Path.Combine("Documents", uniqueFileName),
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                ExtractedText = extractedText
            };

            await _documentRepository.AddAsync(document);
            return ServiceResult<DocumentResponse>.Created(
                ToResponse(document),
                "Document uploaded successfully.");
        }
        catch (Exception exception)
        {
            TryDeleteFile(physicalPath);
            _logger.LogError(exception, "Document upload failed for employee {EmployeeId}.", employeeId);
            return ServiceResult<DocumentResponse>.UnexpectedError(
                "The document could not be uploaded.");
        }
    }

    public async Task<ServiceResult<DocumentResponse>> UpdateAsync(
        int id,
        DocumentUpdateDto dto,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        var existingDocument = await _documentRepository.GetByIdAsync(id, employeeId);
        if (existingDocument == null)
            return ServiceResult<DocumentResponse>.NotFound("Document not found.");

        var errors = await _fileValidationService.ValidateAsync(dto.File, cancellationToken);
        if (errors.Count > 0)
        {
            return ServiceResult<DocumentResponse>.ValidationError(
                "File validation failed.",
                errors);
        }

        var file = dto.File!;
        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var documentsPath = Path.Combine(_environment.ContentRootPath, "Documents");
        Directory.CreateDirectory(documentsPath);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var newPhysicalPath = Path.Combine(documentsPath, uniqueFileName);
        var oldPhysicalPath = Path.Combine(_environment.ContentRootPath, existingDocument.FilePath);

        try
        {
            await SaveFileAsync(file, newPhysicalPath, cancellationToken);
            var extractedText = ExtractText(newPhysicalPath, extension);

            existingDocument.FileName = originalFileName;
            existingDocument.ContentType = file.ContentType;
            existingDocument.FileSize = file.Length;
            existingDocument.FilePath = Path.Combine("Documents", uniqueFileName);
            existingDocument.ExtractedText = extractedText;

            await _documentRepository.UpdateAsync(existingDocument);
        }
        catch (Exception exception)
        {
            TryDeleteFile(newPhysicalPath);
            _logger.LogError(
                exception,
                "Document update failed for document {DocumentId} and employee {EmployeeId}.",
                id,
                employeeId);
            return ServiceResult<DocumentResponse>.UnexpectedError(
                "The document could not be updated.");
        }

        TryDeleteFile(oldPhysicalPath);
        return ServiceResult<DocumentResponse>.Success(
            ToResponse(existingDocument),
            "Document updated successfully.");
    }

    public async Task<ServiceResult<object?>> DeleteAsync(int id, int employeeId)
    {
        var document = await _documentRepository.GetByIdAsync(id, employeeId);
        if (document == null)
            return ServiceResult<object?>.NotFound("Document not found.");

        try
        {
            await _documentRepository.DeleteAsync(document);
            TryDeleteFile(Path.Combine(_environment.ContentRootPath, document.FilePath));
            return ServiceResult<object?>.Success(null, "Document deleted successfully.");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Document deletion failed for document {DocumentId} and employee {EmployeeId}.",
                id,
                employeeId);
            return ServiceResult<object?>.UnexpectedError(
                "The document could not be deleted.");
        }
    }

    public async Task<ServiceResult<IReadOnlyList<DocumentResponse>>> SearchAsync(
        string query,
        int employeeId)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ServiceResult<IReadOnlyList<DocumentResponse>>.ValidationError(
                "Search query is required.",
                ["The q query-string parameter cannot be empty."]);
        }

        var documents = await _documentRepository.SearchAsync(query.Trim(), employeeId);
        var response = documents.Select(ToResponse).ToList();
        return ServiceResult<IReadOnlyList<DocumentResponse>>.Success(
            response,
            "Search completed successfully.");
    }

    private static async Task SaveFileAsync(
        IFormFile file,
        string physicalPath,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);
    }

    private string ExtractText(string physicalPath, string extension)
    {
        if (extension != ".pdf")
            return _ocrService.ExtractTextFromImage(physicalPath);

        var extractedText = _pdfTextExtractor.ExtractText(physicalPath);
        if (!string.IsNullOrWhiteSpace(extractedText) && extractedText.Trim().Length >= 20)
            return extractedText;

        var ocrDirectory = Path.Combine(
            _environment.ContentRootPath,
            "TempOCR",
            Guid.NewGuid().ToString());
        Directory.CreateDirectory(ocrDirectory);

        try
        {
            var pageText = _pdfRenderer.RenderPages(physicalPath, ocrDirectory)
                .Select(_ocrService.ExtractTextFromImage);
            return string.Join(Environment.NewLine, pageText);
        }
        finally
        {
            if (Directory.Exists(ocrDirectory))
                Directory.Delete(ocrDirectory, recursive: true);
        }
    }

    private void TryDeleteFile(string physicalPath)
    {
        try
        {
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not delete file {FilePath}.", physicalPath);
        }
    }

    private static DocumentResponse ToResponse(Document document)
    {
        var text = document.ExtractedText;
        if (text?.Length > ResponseTextLimit)
            text = text[..ResponseTextLimit];

        return new DocumentResponse
        {
            Id = document.Id,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            UploadedAt = document.UploadedAt,
            ExtractedText = text
        };
    }
}
