using DocumentSearch.Helpers;
using DocumentSearch.Services.Interfaces;
using DocumentSearch.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentSearch.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!User.TryGetEmployeeId(out var employeeId))
            return InvalidEmployeeClaim();

        return ApiHelper.ToActionResult(
            await _documentService.GetAllAsync(employeeId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!User.TryGetEmployeeId(out var employeeId))
            return InvalidEmployeeClaim();

        return ApiHelper.ToActionResult(
            await _documentService.GetByIdAsync(id, employeeId));
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
        [FromForm] DocumentUploadDto dto,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetEmployeeId(out var employeeId))
            return InvalidEmployeeClaim();

        return ApiHelper.ToActionResult(
            await _documentService.UploadAsync(dto, employeeId, cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] DocumentUpdateDto dto,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetEmployeeId(out var employeeId))
            return InvalidEmployeeClaim();

        return ApiHelper.ToActionResult(
            await _documentService.UpdateAsync(id, dto, employeeId, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.TryGetEmployeeId(out var employeeId))
            return InvalidEmployeeClaim();

        return ApiHelper.ToActionResult(
            await _documentService.DeleteAsync(id, employeeId));
    }

    private static IActionResult InvalidEmployeeClaim() =>
        ApiHelper.ToActionResult(
            ServiceResult<object?>.Unauthorized(
                "Employee ID could not be read from the token."));
}
