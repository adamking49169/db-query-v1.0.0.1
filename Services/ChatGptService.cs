using System.Net.Http.Json;
using System.Text;

namespace db_query_v1._0._0._1.Services
{
    public class ChatGptService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ChatGptService(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<string> GetChatGptResponse(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4", // or "gpt-3.5-turbo"
                messages = new[]
                {
                new { role = "user", content = prompt }
            }
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ChatGptResponse>();
            return result.Choices[0].Message.Content;
        }

        private class ChatGptResponse
        {
            public Choice[] Choices { get; set; }
        }

        private class Choice
        {
            public Message Message { get; set; }
        }

        private class Message
        {
            public string Content { get; set; }
        }
    }
}
