namespace DocumentSearch.ViewModels;

public sealed class DocumentResponse
{
    public int Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTime UploadedAt { get; init; }
    public string? ExtractedText { get; init; }
}
