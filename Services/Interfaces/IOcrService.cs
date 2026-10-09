namespace DocumentSearch.Services.Interfaces
{
    public interface IOcrService
    {
        string ExtractTextFromImage(string imagePath);
    }
}
