using HarfBuzzSharp;
using Microsoft.Extensions.Hosting;
using System.IO;
using Tesseract;

namespace db_query_v1._0._0._1.Services
{
    public class OcrService
    {
        private readonly string _tessdataPath;

        public OcrService(IHostEnvironment env)
        {
            _tessdataPath = Path.Combine(env.ContentRootPath, "tessdata");
        }
        public string ExtractTextFromImage(string imagePath)
        {
            using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        var b = page.GetText();
                        return page.GetText();
                    }
                }
            }
        }
    }

}
