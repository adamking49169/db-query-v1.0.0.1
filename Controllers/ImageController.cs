using db_query_v1._0._0._1.Services;
using Microsoft.AspNetCore.Mvc;
using db_query_v1._0._0._1.Models;

namespace db_query_v1._0._0._1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly OcrService _ocrService;
        private readonly ChatGptService _chatGptService;
        private readonly ImageGenerationService _imageGenerationService;

        public ImageController(OcrService ocrService,
                               ChatGptService chatGptService,
                               ImageGenerationService imageGenerationService)
        {
            _ocrService = ocrService;
            _chatGptService = chatGptService;
            _imageGenerationService = imageGenerationService;
        }

        [HttpPost("extract-and-process")]
        public async Task<IActionResult> ExtractAndProcessImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Save the uploaded image to a temporary file
            string extractedText;
            using (var stream = file.OpenReadStream())
            {
                extractedText = _ocrService.ExtractTextFromImage(stream);
            }


            // Process the extracted text using ChatGPT
            var chatGptResponse = extractedText;


            return Ok(new
            {
                ExtractedText = extractedText,
                ChatGptResponse = chatGptResponse
            });
        }

        [HttpPost("generate-from-prompt")]
        public async Task<IActionResult> GenerateFromPrompt([FromBody] ImagePrompt request)
        {
            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return BadRequest("Prompt is required.");
            }

            var imageUrl = await _imageGenerationService.GenerateImageAsync(request.Prompt);
            return Ok(new { ImageUrl = imageUrl });
        }
    }
}
