# DocumentSearch

A .NET 9 ASP.NET Core web API for storing uploaded documents and extracting their text. Supports PDF, JPEG and PNG files, stores metadata in SQL Server, and uses OCR for scanned or image-based documents.

> **Scope:** upload, retrieval, replacement, deletion and text extraction. There is no full-text search endpoint and no authentication yet.

## Tech stack

- .NET 9, ASP.NET Core Web API, Entity Framework Core 9, SQL Server
- Swagger (Swashbuckle) and Microsoft.AspNetCore.OpenApi
- PdfPig for PDF text, PDFtoImage + SkiaSharp to render PDF pages, Tesseract for OCR (English)

## Architecture

Controller -> Service -> Repository

```text
Controllers/   DocumentsController
Services/      DocumentService, OcrService, PdfExtractorService, PdfRenderer
Repository/    DocumentRepository
Models/        Document, Employee
ViewModels/    DocumentUploadDto, DocumentUpdateDto
Data/          ApplicationDbContext
```

## How it works

1. The client uploads a file with `UploadedByEmployeeId` (multipart request).
2. The service validates the file and saves it to `Documents/` under a GUID file name.
3. Text is extracted:
   - **PDF:** PdfPig first. If fewer than 20 characters come back, the PDF is treated as scanned: each page is rendered to a temporary image, OCR'd with Tesseract, and the temp images are deleted.
   - **JPEG/PNG:** sent directly to Tesseract.
4. Metadata and `ExtractedText` are saved through EF Core.

## API

| Method | Route | Description |
|---|---|---|
| GET | `/api/documents` | List all documents |
| GET | `/api/documents/{id}` | Get one document (404 if missing) |
| POST | `/api/documents/upload` | Upload and extract text |
| PUT | `/api/documents/{id}` | Replace file, refresh metadata and text |
| DELETE | `/api/documents/{id}` | Delete record and physical file |

```powershell
curl.exe -X POST -F "File=@C:\path\to\example.pdf" -F "UploadedByEmployeeId=1" http://localhost:5154/api/documents/upload
```

## Database

- **T_Employees:** `Id`, `EmployeeCode`, `FirstName`, `LastName`
- **T_Documents:** `Id`, `UploadedByEmployeeId`, `FileName`, `ContentType`, `FilePath`, `FileSize`, `UploadedAt`, `ExtractedText`
- Delete behavior is restricted: an employee cannot be deleted while documents reference them.

## Setup

1. Install the .NET 9 SDK and SQL Server.
2. Set `ConnectionStrings:DefaultConnection` in `appsettings.json`.
3. Make sure `tessdata/eng.traineddata` is in the project and copied to the output folder.
4. Run:

```powershell
dotnet restore
dotnet ef database update
dotnet run
```

Dev URLs: `http://localhost:5154` and `https://localhost:7065`. Swagger UI is served at the root; the OpenAPI document is at `/openapi/v1.json`.

