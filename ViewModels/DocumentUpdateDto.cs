using System.ComponentModel.DataAnnotations;

namespace DocumentSearch.ViewModels;

public sealed class DocumentUpdateDto
{
    [Required]
    public IFormFile? File { get; set; }
}
