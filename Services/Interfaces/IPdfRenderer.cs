namespace DocumentSearch.Services.Interfaces
{
    public interface IPdfRenderer
    {
        List<string> RenderPages(string pdfPath, string outputDirectory);
    }
}
