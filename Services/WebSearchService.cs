using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace db_query_v1._0._0._1.Services
{
    public class WebSearchService
    {
        private readonly HttpClient _httpClient;

        public WebSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SearchAsync(string query)
        {
            var url = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(query)}&format=json&no_redirect=1&skip_disambig=1";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);
            var results = new StringBuilder();
            var topics = json["RelatedTopics"] as JArray;
            if (topics != null)
            {
                int count = 0;
                foreach (var topic in topics)
                {
                    var text = topic["Text"]?.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        results.AppendLine("- " + text);
                        if (++count == 3) break;
                    }
                }
            }
            return results.ToString();
        }
    }
}