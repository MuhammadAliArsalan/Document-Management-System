using PDFtoImage;
using SkiaSharp;
using DocumentSearch.Services.Interfaces;

namespace DocumentSearch.Services.Implementationss
{
    public class PdfRenderer : IPdfRenderer
    {
        public List<string> RenderPages(
            string pdfPath,
            string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            var outputFiles = new List<string>();

            using var pdfStream = File.OpenRead(pdfPath);

            var images = Conversion.ToImages(pdfStream);

            int pageNumber = 1;

            foreach (var image in images)
            {
                var outputPath = Path.Combine(
                    outputDirectory,
                    $"page-{pageNumber}.png");

                using (image)
                using (var outputStream = File.Create(outputPath))
                {
                    image.Encode(
                        outputStream,
                        SKEncodedImageFormat.Png,
                        100);
                }

                outputFiles.Add(outputPath);

                pageNumber++;
            }

            return outputFiles;
        }
    }
}