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

namespace YourNamespace.Controllers
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
                    .Select(pc => new PreviousChat { Title = pc.Title, Date = pc.Date })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChatWithData(ChatModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            // 1. Persist user message
            var userMessage = new ChatHistoryItem
            {
                Role = "user",
                Content = model.UserInput,
                Timestamp = DateTime.UtcNow
            };
            _db.ChatHistoryItems.Add(userMessage);

            // 2. Get AI response
            var response = await GetOpenAiResponse(model.UserInput, user);

            // 3. Persist assistant message
            var assistantHistoryItem = new ChatHistoryItem
            {
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
            _db.PreviousChats.Add(new PreviousChat
            {
                Title = summaryTitle,
                Date = DateTime.UtcNow
            });

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
                .OrderByDescending(pc => pc.Date)
                .Take(10)
                .Select(pc => new PreviousChat { Title = pc.Title, Date = pc.Date })
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

        private async Task<string> GetOpenAiResponse(string userInput, ApplicationUser appUser)
        {
            var client = _httpClientFactory.CreateClient("OpenAI");
            _apiRequestCount++;
            Console.WriteLine($"API Request Count: {_apiRequestCount}");

            // Build specialization prompt
            var raw = appUser?.Specializations ?? string.Empty;
            var specs = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            var system = specs.Any()
                ? $"You are an expert in: {string.Join(", ", specs)}. Only answer within these areas."
                : "You are a helpful assistant.";

            var body = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = system },
                    new { role = "user", content = userInput }
                },
                max_tokens = 500,
                temperature = 0.4,
                top_p = 0.9,
                frequency_penalty = 0.2
            };

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var resp = await client.PostAsJsonAsync("v1/chat/completions", body);

                    if (resp.IsSuccessStatusCode)
                    {
                        var result = await resp.Content.ReadFromJsonAsync<OpenAiChatResponse>();
                        return result?.Choices.FirstOrDefault()?.Message.Content.Trim() ?? string.Empty;
                    }
                }
                catch
                {
                    await Task.Delay(1000);
                }
            }
            return "Error generating response.";
        }
    }
}
