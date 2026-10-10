// using UglyToad.PdfPig;
// using DocumentSearch.Services.Interfaces;

// namespace DocumentSearch.Services.Implementationss
// {
//     public class PdfTextExtractorService : IPdfTextExtractor
//     {
//         public string ExtractText(string filePath)
//         {
//             using var document = PdfDocument.Open(filePath);

//             var text = new System.Text.StringBuilder();

//             foreach (var page in document.GetPages())
//             {
//                 text.AppendLine(page.Text);
//             }
//             return text.ToString();
//         }
//     }
// }
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using DocumentSearch.Services.Interfaces;

namespace DocumentSearch.Services.Implementationss
{
    public class PdfTextExtractorService : IPdfTextExtractor
    {
        public string ExtractText(string filePath)
        {
            StringBuilder text = new StringBuilder();

            using (PdfDocument document = PdfDocument.Open(filePath))
            {
                foreach (Page page in document.GetPages())
                {
                    foreach (Word word in page.GetWords())
                    {
                        text.Append(word.Text);
                        text.Append(' ');
                    }

                    text.AppendLine();
                }
            }

            return text.ToString();
        }
    }
}