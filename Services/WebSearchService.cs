using System;
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
            const int maxQueryLength = 200;
            if (query.Length > maxQueryLength)
            {
                var words = query.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                query = string.Join(" ", words.Take(30));
            }
            var url = $"?q={Uri.EscapeDataString(query)}&format=json&no_redirect=1&skip_disambig=1";
            string response;
            try
            {
                response = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                return $"Search error: {ex.Message}";
            }

            JObject json;
            try
            {
                json = JObject.Parse(response);
            }
            catch (Exception ex)
            {
                return $"Search error: {ex.Message}";
            }


            var results = new StringBuilder();

            var topics = json["RelatedTopics"] as JArray;

            // Debug: Inspect tokens with values in RelatedTopics
            var info = topics
                .Where(x => x.HasValues)
                .Select(x => (x.Path, x.Type));

            if (topics != null)
            {
                int count = 0;
                const int maxResults = 10;

                foreach (var topic in topics)
                {
                    // Check if this is a direct topic
                    var text = topic["Text"]?.ToString();
                    var urlItem = topic["FirstURL"]?.ToString();

                    if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(urlItem))
                    {
                        results.AppendLine($"- {text} ({urlItem})");
                        if (++count == maxResults) break;
                        continue;
                    }

                    // Check if this is a category with nested Topics
                    var subTopics = topic["Topics"] as JArray;
                    if (subTopics != null)
                    {
                        foreach (var subTopic in subTopics)
                        {
                            var subText = subTopic["Text"]?.ToString();
                            var subUrl = subTopic["FirstURL"]?.ToString();

                            if (!string.IsNullOrEmpty(subText) && !string.IsNullOrEmpty(subUrl))
                            {
                                results.AppendLine($"- {subText} ({subUrl})");
                                if (++count == maxResults) break;
                            }
                        }
                    }

                    if (count == maxResults) break;
                }
            }

            return string.IsNullOrWhiteSpace(results.ToString()) ? "No results found." : results.ToString();
        }
    }
}