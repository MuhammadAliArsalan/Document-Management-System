using System.ComponentModel.DataAnnotations;

namespace DocumentSearch.ViewModels;

public sealed class DocumentUploadDto
{
    [Required]
    public IFormFile? File { get; set; }
}
