using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

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
            var url = $"search?q={Uri.EscapeDataString(query)}&num=10&hl=en&gl=us&client=firefox-b-d";
            string response;
            try
            {
                response = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                return $"Search error: {ex.Message}";
            }

            var doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(response);
            }
            catch (Exception ex)
            {
                return $"Search error: {ex.Message}";
            }


            var results = new StringBuilder();
            int count = 0;
            const int maxResults = 10;


            var nodes = doc.DocumentNode.SelectNodes("//div[@class='g']//a/h3");
            if (nodes != null)
            {
                foreach (var h3 in nodes)
                {

                    var a = h3.ParentNode;
                    var href = a.GetAttributeValue("href", string.Empty);
                    var text = h3.InnerText.Trim();
                    if (href.StartsWith("/url?q="))
                    {
                        href = href[7..];
                        var idx = href.IndexOf('&');
                        if (idx > 0)
                            href = href[..idx];
                    }

                    if (!string.IsNullOrEmpty(text) && href.StartsWith("http"))
                    {
                       
                            results.AppendLine($"- {text} ({href})");
                            if (++count == maxResults) break;
                    }

                }
            }

            return string.IsNullOrWhiteSpace(results.ToString()) ? "No results found." : results.ToString();
        }
    }
}