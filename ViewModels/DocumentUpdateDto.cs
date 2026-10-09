namespace DocumentSearch.ViewModels
{
    public class DocumentUpdateDto
    {
        public IFormFile File { get; set; } = null!;
        public int UploadedByEmployeeId { get; set; }
    }
}
