using UglyToad.PdfPig;
using DocumentSearch.Services.Interfaces;

namespace DocumentSearch.Services.Implementationss
{
    public class PdfTextExtractorService : IPdfTextExtractor
    {
        public string ExtractText(string filePath)
        {
            using var document = PdfDocument.Open(filePath);

            var text = new System.Text.StringBuilder();

            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }
            return text.ToString();
        }
    }
}
