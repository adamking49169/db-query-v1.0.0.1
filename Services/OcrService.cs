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
            using var img = Pix.LoadFromFile(imagePath);
            using var page = _engine.Process(img);
            return page.GetText();
        }

        public string ExtractTextFromImage(Stream imageStream)
        {
            using var memoryStream = new MemoryStream();
            imageStream.CopyTo(memoryStream);
            return ExtractTextFromImage(memoryStream.ToArray());
        }

        public string ExtractTextFromImage(byte[] imageData)
        {
            using var img = Pix.LoadFromMemory(imageData);
            using var page = _engine.Process(img);
            return page.GetText();
        }
    }

}
