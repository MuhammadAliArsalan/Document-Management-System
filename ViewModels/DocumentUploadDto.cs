using Microsoft.AspNetCore.Http;
namespace DocumentSearch.ViewModels
{
    public class DocumentUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public int UploadedByEmployeeId { get; set; }
    }
}
