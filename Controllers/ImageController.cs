using db_query_v1._0._0._1.Services;
using Microsoft.AspNetCore.Mvc;

namespace db_query_v1._0._0._1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly OcrService _ocrService;
        private readonly ChatGptService _chatGptService;

        public ImageController(OcrService ocrService, ChatGptService chatGptService)
        {
            _ocrService = ocrService;
            _chatGptService = chatGptService;
        }

        [HttpPost("extract-and-process")]
        public async Task<IActionResult> ExtractAndProcessImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the uploaded image to a temporary file
            var tempImagePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempImagePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Extract text from the image
            var extractedText = _ocrService.ExtractTextFromImage(tempImagePath);

            // Process the extracted text using ChatGPT
            var chatGptResponse = extractedText;

            // Clean up the temporary file
            System.IO.File.Delete(tempImagePath);

            return Ok(new
            {
                ExtractedText = extractedText,
                ChatGptResponse = chatGptResponse
            });
        }
    }
}
