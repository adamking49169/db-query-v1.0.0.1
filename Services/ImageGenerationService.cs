using System.Net.Http.Json;
using System.Net.Http.Headers;

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
                prompt = prompt,
                n = 1,
                size = "512x512"
            };

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/images/generations", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DallEResponse>();
            return result?.Data?.FirstOrDefault()?.Url ?? string.Empty;
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
