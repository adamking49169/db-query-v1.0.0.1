using HarfBuzzSharp;
using Microsoft.Extensions.Hosting;
using System.IO;
using Tesseract;

namespace db_query_v1._0._0._1.Services
{
    public class OcrService
    {
        private readonly string _tessdataPath; 
        private readonly TesseractEngine _engine;

        public OcrService(IHostEnvironment env)
        {
            _tessdataPath = Path.Combine(env.ContentRootPath, "tessdata");
            _engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
        }
        public string ExtractTextFromImage(string imagePath)
        {
            using (var img = Pix.LoadFromFile(imagePath))
            {
                using (var page = _engine.Process(img))
                {
                    return page.GetText();
                }
            }
        }
    }

}
