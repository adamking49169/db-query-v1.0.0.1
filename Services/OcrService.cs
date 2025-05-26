using HarfBuzzSharp;
using Tesseract;

namespace db_query_v1._0._0._1.Services
{
    public class OcrService
    {
        public string ExtractTextFromImage(string imagePath)
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
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
