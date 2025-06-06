using db_query_v1._0._0._1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace db_query_v1._0._0._1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebSearchController : ControllerBase
    {
        private readonly WebSearchService _webSearch;

        public WebSearchController(WebSearchService webSearch)
        {
            _webSearch = webSearch;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            var results = await _webSearch.SearchAsync(query);
            return Ok(new { results });
        }
    }
}