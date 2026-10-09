using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DocumentSearch.ViewModels;
using DocumentSearch.Models;
using DocumentSearch.Services.Interfaces;

namespace DocumentSearch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search query is required.");

            var documents = await _documentService.SearchAsync(q);

            return Ok(documents);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var documents = await _documentService.GetAllAsync();

            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            [FromForm] DocumentUploadDto dto)
        {
            try
            {
                var document = await _documentService.UploadAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = document.Id },
                    document);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
               int id,
               [FromForm] DocumentUpdateDto dto)
        {
            try
            {
                var updated = await _documentService.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _documentService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

    }
}
