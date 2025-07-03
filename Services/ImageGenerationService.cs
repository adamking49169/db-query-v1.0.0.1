using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace db_query_v1._0._0._1.Services
{
    public class ImageGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ImageGenerationService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> GenerateImageAsync(string prompt)
        {
            var requestBody = new
            {
                model = "dall-e-2",    
                prompt = prompt,
                n = 1,
                size = "512x512"
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync(
     "https://api.openai.com/v1/images/generations",
     requestBody
 );

            var sentJson = JsonSerializer.Serialize(requestBody);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($" JSON payload: {sentJson}");
            Console.WriteLine($" HTTP {(int)response.StatusCode}: {body}");

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<DallEResponse>();
            return result?.Data?.FirstOrDefault()?.Url
                   ?? string.Empty;
        }

        private class DallEResponse
        {
            public List<ImageData> Data { get; set; }
        }

        private class ImageData
        {
            public string Url { get; set; }
        }
    }
}
