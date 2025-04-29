using System.Diagnostics;
using db_query_v1._0._0._1.Models;
using db_query_v1._9_0._0_1.DataProcessing;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace db_query_v1._0._0._1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Q _q;

        public HomeController(ILogger<HomeController> logger, Q q)
        {
            _logger = logger;
            _q = q;
        }

        public IActionResult DataProcessing()
        {
            var dataProcessor = new DataProcessor();
            string filePath = "Data/titanic.csv";

            // Get the processed data from the method (now returns List<DataRow>)
            var processedData = dataProcessor.ProcessCsvData(filePath);

            // Create the ChatModel and assign the data
            var chatModel = new ChatModel
            {
                Data = processedData
            };

            return View("Index", chatModel);
        }// Pass the model to the view

        public IActionResult Index()
        {

            {
                var model = new ChatModel
                {
                    PasscodeValidated = true,
                    UsingDummyData = true,
                    Passcode = "dummy-passcode",
                    UserInput = "Show me sales data",
                    ChatHistory = new List<ChatHistoryItem>
                              {
                                  new ChatHistoryItem { Role = "user", Content = "Hello, what can you do?" },
                                  new ChatHistoryItem { Role = "assistant", Content = "I can help you query your database or visualize your data." },
                                  new ChatHistoryItem { Role = "user", Content = "Show me sales data for Q1." }
                              },
                    Data = new List<DataRow>
                       {
                           new DataRow { ColumnName = "Region", Value = "North America" },
                           new DataRow { ColumnName = "Sales", Value = "$120,000" },
                           new DataRow { ColumnName = "Quarter", Value = "Q1" }
                       }
                };

                return View(model);

            }
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
