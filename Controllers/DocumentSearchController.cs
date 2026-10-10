using DocumentSearch.Helpers;
using DocumentSearch.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentSearch.Controllers;

[Authorize]
[Route("api/documents/search")]
[ApiController]
public class DocumentSearchController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentSearchController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q)
    {
        if (!User.TryGetEmployeeId(out var employeeId))
        {
            return ApiHelper.ToActionResult(
                ServiceResult<object?>.Unauthorized(
                    "Employee ID could not be read from the token."));
        }

        return ApiHelper.ToActionResult(
            await _documentService.SearchAsync(q ?? string.Empty, employeeId));
    }
}
