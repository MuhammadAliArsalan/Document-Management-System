using DocumentSearch.Services.Interfaces;
using Tesseract;
namespace DocumentSearch.Services.Implementationss
{
    public class OcrService : IOcrService
    {
        private readonly IWebHostEnvironment _environment;
        public OcrService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string ExtractTextFromImage(string imagePath)
        {
            var tessDataPath = Path.Combine(
                _environment.ContentRootPath,
                "tessdata");

            using var engine = new TesseractEngine(
                tessDataPath,
                "eng",
                EngineMode.Default);

            using var image = Pix.LoadFromFile(imagePath);

            using var page = engine.Process(image);

            return page.GetText();
        }
    }
}
