using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace db_query_v1._0._0._1.Services
{
    public class WebSearchService
    {
        private readonly HttpClient _httpClient;

        // Required for AddHttpClient<T>()
        public WebSearchService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> SearchAsync(string query)
        {
            // Limit long queries
            const int maxQueryLength = 200;
            if (query.Length > maxQueryLength)
            {
                var words = query.Split(new[] { ' ', '\t', '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries);
                query = string.Join(" ", words[..Math.Min(words.Length, 30)]);
            }

            // Build search URL (news vertical gives simple HTML)
            var url = $"/search?q={Uri.EscapeDataString(query)}&num=10&hl=en&gl=us&tbm=nws";
            string response;
            try
            {
                response = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                return $"Search error: {ex.Message}";
            }

            // Parse HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var results = new StringBuilder();
            int count = 0;
            const int maxResults = 10;

            var nodes = doc.DocumentNode.SelectNodes("//a[contains(@href,'/url?q=')]");
            if (nodes != null)
            {
                foreach (var a in nodes)
                {
                    var href = a.GetAttributeValue("href", string.Empty);
                    if (href.StartsWith("/url?q="))
                    {
                        href = href[7..];
                        var idx = href.IndexOf('&');
                        if (idx > 0) href = href[..idx];
                    }

                    var titleNode = a.SelectSingleNode(".//h3") ??
                                    a.SelectSingleNode(".//div[contains(@class,'ilUpNd')]");
                    var text = WebUtility.HtmlDecode(titleNode?.InnerText.Trim() ?? "");

                    if (!string.IsNullOrEmpty(text) && Uri.IsWellFormedUriString(href, UriKind.Absolute))
                    {
                        results.AppendLine($"- {text} ({href})");
                        if (++count == maxResults) break;
                    }
                }
            }

            return string.IsNullOrWhiteSpace(results.ToString())
                ? "No results found."
                : results.ToString();
        }
    }
}
