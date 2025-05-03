using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using db_query_v1._0._0._1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Microsoft.Data.SqlClient;
using db_query_v1._0._0._1.Data;
using Microsoft.AspNetCore.Authorization;
using db_query_v1._9_0._0_1.DataProcessing;
using Newtonsoft.Json;
using System.Text;
using static db_query_v1._0._0._1.Data.ApplicationDbContext;

namespace db_query_v1._0._0._1.Controllers
{
    public class ChatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private int _apiRequestCount = 0;

        public ChatController(
            IHttpClientFactory httpClientFactory,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _db = db;
        }

        public IActionResult DataProcessing()
        {
            var dataProcessor = new DataProcessor();
            string filePath = @"C:\Users\adam\source\repos\db-query-v1.0.0.1\Data\titanic.csv"; // Full path to the CSV file

            // Get the processed data from the method (now returns List<DataRow>)
            var processedData = dataProcessor.ProcessCsvData(filePath);

            // Create the ChatModel and assign the data
            var chatModel = new ChatModel
            {
                Data = processedData
            };

            return View("ChatWithData", chatModel);  // Pass the model to the view
        }


        public IActionResult QueryData(string userInput)
        {
            var dataProcessor = new DataProcessor();
            string filePath = @"C:\Users\adam\source\repos\db-query-v1.0.0.1\Data\titanic.csv"; // Full path to the CSV file

            // Get the processed data
            var processedData = dataProcessor.ProcessCsvData(filePath);

            // Query the data based on user input
            var filteredData = processedData
                .Where(row => row.ColumnName.Contains(userInput, StringComparison.OrdinalIgnoreCase) ||
                              row.Value.Contains(userInput, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var chatModel = new ChatModel
            {
                Data = filteredData,
                UserInput = userInput
            };

            return View("ChatWithData", chatModel);  // Render the updated data
        }


        [HttpGet]
        public IActionResult ChatWithData()
        {
            var model = new ChatModel
            {
                PasscodeValidated = true,
                UsingDummyData = false,
                UserInput = string.Empty,
                ChatHistory = new List<ChatHistoryItem>(),
                Data = new List<DataRow>(),
                PreviousChats = _db.PreviousChats
                    .OrderByDescending(pc => pc.Date)
                    .Take(10)
                    .Select(pc => new PreviousChat { Title = pc.Title, Date = pc.Date, UserIdentityId = pc.UserIdentityId })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChatWithData(ChatModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            var userId = _userManager.GetUserId(User);

            var userMessage = new ChatHistoryItem
            {
                UserIdentityId = userId,
                Role = "user",
                Content = model.UserInput,
                Timestamp = DateTime.UtcNow
            };
            _db.ChatHistoryItems.Add(userMessage);

            // 2. Get AI response
            var response = await GetOpenAiResponseStreamed(model.UserInput, user);

            // 3. Persist assistant message
            var assistantHistoryItem = new ChatHistoryItem
            {
                UserIdentityId = userId,
                Role = "assistant",
                Content = response,
                Timestamp = DateTime.UtcNow
            };
            _db.ChatHistoryItems.Add(assistantHistoryItem);

            // 4. Log to ChatResponseLog
            var logEntry = new ChatResponseLog
            {
                Role = assistantHistoryItem.Role,
                Content = assistantHistoryItem.Content,
                Timestamp = assistantHistoryItem.Timestamp
            };
            _db.ChatResponseLogs.Add(logEntry);

            // 5. Add session summary
            var summaryTitle = model.UserInput.Length > 50
                ? model.UserInput[..50] + "..."
                : model.UserInput;

            var previousChatEntry = new PreviousChat
            {
                UserIdentityId = userId,
                Title = summaryTitle,
                Date = DateTime.UtcNow
            };
            _db.PreviousChats.Add(previousChatEntry);

            // 6. Save messaging and summary
            await _db.SaveChangesAsync();

            // 7. Rebuild history in model
            model.ChatHistory ??= new List<ChatHistoryItem>();
            model.ChatHistory.Add(userMessage);
            model.ChatHistory.Add(assistantHistoryItem);
            if (model.ChatHistory.Count > 20)
                model.ChatHistory = model.ChatHistory.Skip(model.ChatHistory.Count - 20).ToList();

            // 8. Reload previous chats
            model.PreviousChats = _db.PreviousChats
       .Where(pc => pc.UserIdentityId == userId)  // Filter by UserIdentityId
       .OrderByDescending(pc => pc.Date)  // Order by Date in descending order
       .Take(10)  // Take the top 10 results
       .Select(pc => new PreviousChat { Title = pc.Title, Date = pc.Date, UserIdentityId = pc.UserIdentityId })  // Project to PreviousChat
       .ToList();

            // 9. Populate model.Data only if valid SELECT query
            model.Data = new List<DataRow>();
            var sql = model.UserInput?.Trim();
            if (!string.IsNullOrEmpty(sql) && sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var connection = _db.Database.GetDbConnection();
                    await connection.OpenAsync();
                    using var command = connection.CreateCommand();
                    command.CommandText = sql;

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            model.Data.Add(new DataRow
                            {
                                ColumnName = reader.GetName(i),
                                Value = reader.GetValue(i)?.ToString() ?? string.Empty
                            });
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Log or add model error
                    ModelState.AddModelError("Data", "SQL error: " + ex.Message);
                }
            }

            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage(ChatModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Get the current user's Id
            var userId = _userManager.GetUserId(User);
            // Alternatively: var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 2) Create & populate the ChatHistoryItem
            var historyItem = new ChatHistoryItem
            {
                UserIdentityId = userId,         // ← THIS is _required_
                Role = "user",
                Content = model.UserInput,
                Timestamp = DateTime.UtcNow
            };

            // 3) Save it
            _db.ChatHistoryItems.Add(historyItem);
            await _db.SaveChangesAsync();

            // ... your code to generate the assistant response, etc.

            return RedirectToAction("Index", new { /* … */ });
        }
        private async Task<string> GetOpenAiResponseStreamed(string userInput, ApplicationUser appUser)
        {
            var client = _httpClientFactory.CreateClient("OpenAI");

            // Read and process Titanic CSV data if Data Analytics specialization is detected
            var specs = (appUser?.Specializations ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var systemMessage = "";

            if (specs.Contains("Data Analytics"))
            {
                // Read and process Titanic data
                var titanicData = TitanicCsvReader.ReadTitanicCsv(@"C:\Users\adam\source\repos\db-query-v1.0.0.1\Data\titanic.csv");

                systemMessage = "You are a Data Consultant working with a Data Analyst. You will receive a CSV file and use it to answer the user's question. " +
                                "You must decide how to use the data to answer the user's question. Format the output and plot the data to maximize readability. " +
                                "Use appropriate units, and ensure the output is user-friendly.";
            }
            else
            {
                systemMessage = specs.Any()
                    ? $"You are an expert in: {string.Join(", ", specs)}. Only answer within these areas."
                    : "You are a helpful assistant.";
            }

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = systemMessage },
            new { role = "user", content = userInput }
        },
                max_tokens = 300,
                temperature = 0.4,
                stream = true
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
            {
                Content = JsonContent.Create(requestBody)
            };
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var sb = new StringBuilder();
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;
                var json = line["data: ".Length..];
                if (json.Trim() == "[DONE]")
                    break;

                var chunk = JsonConvert.DeserializeObject<OpenAiStreamChunk>(json);
                sb.Append(chunk.Choices[0].Delta.Content);
            }

            return sb.ToString();
        }



    }
}
