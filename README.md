# DocumentSearch

DocumentSearch is a .NET 9 ASP.NET Core Web API for securely uploading, storing, processing, and searching documents. Employees authenticate with JSON Web Tokens (JWT), and every document operation is restricted to the employee who owns the document.

The API extracts text directly from text-based PDFs. For scanned PDFs and image files, it uses Tesseract OCR. Extracted text is stored in SQL Server and can be searched through an authenticated endpoint.

## Features

- Administrator-controlled employee registration and employee login
- Secure password hashing with ASP.NET Core Identity's `PasswordHasher`
- JWT bearer authentication
- Employee-scoped document access
- PDF, JPEG, and PNG uploads with a configurable 20 MB limit
- Extension, MIME type, filename, and file-signature validation
- Direct text extraction from text-based PDFs
- OCR fallback for scanned PDFs
- OCR extraction from JPEG and PNG images
- Document listing and retrieval
- Text-based document search
- Document replacement with refreshed extracted text
- Document and physical-file deletion
- Consistent API response envelopes across authentication and document endpoints
- Swagger/OpenAPI documentation in development
- Entity Framework Core migrations for SQL Server

## How document processing works

1. An administrator registers an employee, and the employee logs in to receive a JWT.
2. The client uploads a document using a multipart form request and includes the JWT in the `Authorization` header.
3. The API obtains the employee ID from the token; document ownership is never taken from the submitted form data.
4. The uploaded file is stored in `Documents/` using a generated GUID filename.
5. Text is extracted according to the file type:
   - **Text-based PDF:** PdfPig reads the text embedded in the PDF.
   - **Scanned PDF:** if PdfPig extracts fewer than 20 characters, each page is rendered as an image and processed with Tesseract OCR.
   - **JPEG or PNG:** the image is processed directly with Tesseract OCR.
6. Metadata and extracted text are saved in SQL Server.
7. Temporary page images created during PDF OCR are removed after processing.

## Architecture

The application follows a layered controller-service-repository architecture:

```text
HTTP Client
    |
    v
Controllers
|-- AuthController
`-- DocumentsController
    |
    v
Services
|-- EmployeeRegistration
|-- EmployeeLogin
|-- TokensService
|-- DocumentService
|-- PdfTextExtractorService
|-- PdfRenderer
`-- OcrService
    |
    |----------------------|
    v                      v
Repositories          Local filesystem
    |                  Documents/ and TempOCR/
    v
Entity Framework Core
    |
    v
SQL Server
```

### Project structure

```text
Controllers/     HTTP endpoints and API response handling
Data/            Entity Framework Core database context
Helpers/         ClaimsPrincipal helper methods
Migrations/      Entity Framework Core database migrations
Models/          Database entities and JWT settings
Repository/      Database access interfaces and implementations
Services/        Business logic, authentication, extraction, and OCR
ViewModels/      Request and response DTOs
Documents/       Locally stored uploaded documents
TempOCR/         Temporary images generated during PDF OCR
tessData/        Tesseract English language data
```

## API endpoints

### Authentication

Login is anonymous. Registration requires a valid JWT containing the `Admin` role.

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register a non-admin employee; administrators only |
| `POST` | `/api/auth/login` | Authenticate an employee and return a JWT |

#### Register

```http
POST /api/auth/register
Content-Type: application/json
Authorization: Bearer <admin-jwt-token>
```

```json
{
  "employeeCode": "EMP001",
  "firstName": "Jane",
  "lastName": "Doe",
  "password": "secure-password"
}
```

#### Login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "employeeCode": "EMP001",
  "password": "secure-password"
}
```

The response contains a token:

```json
{
  "token": "<jwt-token>"
}
```

### Documents

All document endpoints require the following header:

```http
Authorization: Bearer <jwt-token>
```

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/documents` | List documents owned by the authenticated employee |
| `GET` | `/api/documents/{id}` | Retrieve one owned document by ID |
| `GET` | `/api/documents/search?q={query}` | Search the extracted text of owned documents |
| `POST` | `/api/documents/upload` | Upload a PDF, JPEG, or PNG and extract its text |
| `PUT` | `/api/documents/{id}` | Replace an owned document and regenerate its extracted text |
| `DELETE` | `/api/documents/{id}` | Delete an owned document and its physical file |

#### Upload example

```powershell
curl.exe -X POST `
  -H "Authorization: Bearer <jwt-token>" `
  -F "File=@C:\path\to\example.pdf" `
  http://localhost:5154/api/documents/upload
```

#### Search example

```powershell
curl.exe `
  -H "Authorization: Bearer <jwt-token>" `
  "http://localhost:5154/api/documents/search?q=invoice"
```

Search currently performs a substring match against the extracted text stored in SQL Server.

Uploads and replacements accept only PDF, JPEG, and PNG files. The default maximum file size is 20 MB. The API validates the filename, extension, declared MIME type, and file signature before storing or processing a file.

## Document data

Each database document contains:

- `Id`
- `UploadedByEmployeeId`
- Original `FileName`
- `ContentType`
- Relative `FilePath`
- `FileSize`
- `UploadedAt` timestamp
- `ExtractedText`

Employees can access only documents whose `UploadedByEmployeeId` matches the employee ID in their JWT.

API responses use `DocumentResponse` and do not expose the internal `FilePath`, employee navigation property, or ownership field. Extracted text is limited to the first 1,000 characters in responses.

## API response format

Authentication, document, search, validation, and authorization responses use a common envelope:

```json
{
  "success": true,
  "message": "Documents retrieved successfully.",
  "data": [],
  "errors": null
}
```

## Technology stack

- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9
- Microsoft SQL Server
- JWT bearer authentication
- ASP.NET Core Identity password hashing
- PdfPig for embedded PDF text extraction
- PDFtoImage and SkiaSharp for rendering scanned PDF pages
- Tesseract OCR with English language data
- Swagger/Swashbuckle and OpenAPI

## Database

The application uses two principal tables:

### `T_Employees`

- `Id`
- `EmployeeCode` — unique
- `FirstName`
- `LastName`
- `PasswordHash`
- `IsAdmin`

### `T_Documents`

- `Id`
- `UploadedByEmployeeId` — foreign key to `T_Employees`
- `FileName`
- `ContentType`
- `FilePath`
- `FileSize`
- `UploadedAt`
- `ExtractedText`

Employee deletion is restricted while documents still reference the employee.

### Provisioning the first administrator

The `IsAdmin` migration defaults every existing and new employee to `false`. After applying the migration, promote one verified existing employee directly in SQL Server:

```sql
UPDATE T_Employees
SET IsAdmin = 1
WHERE EmployeeCode = 'YOUR_VERIFIED_EMPLOYEE_CODE';
```

Verify that exactly one intended row was updated. That employee must log in again to receive a new JWT containing the `Admin` role. API registration always creates non-admin employees; additional administrator changes are performed explicitly in SQL Server.

## Configuration

Configure the SQL Server connection and JWT settings through `appsettings.json`, development user-secrets, environment variables, or a production secret store.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<sql-server-connection-string>"
  },
  "Jwt": {
    "Key": "<base64-encoded-signing-key>",
    "Issuer": "DocumentSearchApi",
    "Audience": "DocumentSearchApi",
    "ExpiryMinutes": 60
  }
}
```

Do not commit production connection strings or JWT signing keys to source control.

## Prerequisites

- .NET 9 SDK
- SQL Server
- Entity Framework Core CLI tools for applying migrations
- Native dependencies required by Tesseract, PDFtoImage, and SkiaSharp on the target operating system

The English Tesseract language file must be available at `tessData/eng.traineddata` and copied to the application output directory.

## Running locally

Restore packages:

```powershell
dotnet restore
```

Apply database migrations:

```powershell
dotnet ef database update
```

Run the API:

```powershell
dotnet run
```

Development URLs are configured as:

- `http://localhost:5154`
- `https://localhost:7065`

In the development environment, Swagger UI is available at `/swagger` and the generated OpenAPI document is available at `/openapi/v1.json`.

## Current scope

The project currently provides authenticated document storage, extraction, OCR, and basic text search. It does not yet provide a document download endpoint, advanced full-text ranking, pagination, background OCR jobs, role-based authorization, or cloud/object storage.
