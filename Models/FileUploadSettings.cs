namespace DocumentSearch.Models;

public class FileUploadSettings
{
    public const string SectionName = "FileUpload";
    public long MaxFileSizeBytes { get; set; } = 20 * 1024 * 1024;
}
