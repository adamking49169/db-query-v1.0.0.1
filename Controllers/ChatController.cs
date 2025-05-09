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
       
        private  List<PreviousChat> ChatSummaries()
        {
            return _db.PreviousChats
                .OrderByDescending(c => c.Date)
                .Select(c => new PreviousChat
                {
                    Title = c.Title,
                    Date = c.Date,
                    UserIdentityId = c.UserIdentityId,
                    Id = c.Id
                })
                .ToList();
        }

        private Dictionary<int, List<ChatHistoryItem>> ChatHistories()
        {
            var chatHistories = _db.ChatHistoryItems
                .Include(c => c.PreviousChat)  // Ensure we get the related PreviousChat data
                .OrderBy(c => c.Timestamp)     // Sort messages by timestamp
                .ToList();

            // Group chat history items by ChatId
            var groupedHistories = chatHistories
                .GroupBy(c => c.ChatId)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList()
                );

            return groupedHistories;
        }



        // ③ Show the list of previous chats
        public IActionResult Index()
        {
            var model = new ChatModel
            {
                PreviousChats = ChatSummaries() // Call the method using parentheses to invoke it
                                                .OrderByDescending(c => c.Date)
                                                .ToList()
            };
            return View(model);
        }

        // ④ View one chat (full history) on its own page
        [HttpGet]
        public async Task<IActionResult> ViewChat(int id)
        {
            // 1. Get current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 2. Load the requested PreviousChat + its history, ensure it belongs to this user
            var previousChat = await _db.PreviousChats
                .Include(pc => pc.ChatHistoryItems)
                .FirstOrDefaultAsync(pc =>
                    pc.Id == id &&
                    pc.UserIdentityId == user.Id
                );

            if (previousChat == null)
                return NotFound();

            // 3. Build the same ChatModel that ChatWithData expects
            var model = new ChatModel
            {
                // Sort history oldest→newest
                ChatHistory = previousChat.ChatHistoryItems
                                           .OrderBy(c => c.Timestamp)
                                           .ToList(),

                // No ad-hoc SQL when simply viewing
                Data = new List<DataRow>(),

                // Sidebar: latest 10 sessions
                PreviousChats = await _db.PreviousChats
                    .Where(pc => pc.UserIdentityId == user.Id)
                    .OrderByDescending(pc => pc.Date)
                    .Take(10)
                    .ToListAsync()
            };

            // 4. Render the ChatWithData.cshtml view
            return View("ChatWithData", model);
        }

        // ⑤ AJAX endpoint to fetch one chat’s partial
        public IActionResult GetChat(int id)
        {
            if (!ChatHistories().TryGetValue(id, out var messages))
                return PartialView("_NotFound");

            var model = new ChatModel
            {
                ChatHistory = messages,
                Data = new List<DataRow>()
            };
            return PartialView("_ChatDetails", model);
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
                    .Select(pc => new PreviousChat { Title = pc.Title, Date = pc.Date, UserIdentityId = pc.UserIdentityId, Id = pc.Id })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChatWithData(ChatModel model)
        {
            // Get current user
            var user = await _userManager.GetUserAsync(User);
            var userId = _userManager.GetUserId(User);

            // Build a summary title for this session
            var summaryTitle = model.UserInput.Length > 50
                ? model.UserInput[..50] + "..."
                : model.UserInput;

            // Create and track the session (PreviousChat) _before_ the messages
            var previousChatEntry = new PreviousChat
            {
                UserIdentityId = userId,
                Title = summaryTitle,
                Date = DateTime.UtcNow
            };
            _db.PreviousChats.Add(previousChatEntry);

            // Create and track the user's message, linking to the session
            var userMessage = new ChatHistoryItem
            {
                UserIdentityId = userId,
                PreviousChat = previousChatEntry,  // <-- EF will set ChatId for us
                Role = "user",
                Content = model.UserInput,
                Timestamp = DateTime.UtcNow,
                ImageUrl = ""
            };
            _db.ChatHistoryItems.Add(userMessage);

            // Fetch the AI response
            var aiResponse = await GetOpenAiResponseStreamed(model.UserInput, user);

            // Create and track the assistant's reply, also linked to the same session
            var assistantMessage = new ChatHistoryItem
            {
                UserIdentityId = userId,
                PreviousChat = previousChatEntry,
                Role = "assistant",
                Content = aiResponse,
                Timestamp = DateTime.UtcNow,
                ImageUrl = ""
            };
            _db.ChatHistoryItems.Add(assistantMessage);

            // Log the assistant reply to ChatResponseLog (unchanged)
            var logEntry = new ChatResponseLog
            {
                Role = assistantMessage.Role,
                Content = assistantMessage.Content,
                Timestamp = assistantMessage.Timestamp
            };
            _db.ChatResponseLogs.Add(logEntry);

            // Optionally run a SELECT and populate model.Data
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
                    ModelState.AddModelError("Data", "SQL error: " + ex.Message);
                }
            }

            // Commit all of the above in one shot
            await _db.SaveChangesAsync();

            // Rebuild the in-memory history for the view
            model.ChatHistory ??= new List<ChatHistoryItem>();
            model.ChatHistory.Add(userMessage);
            model.ChatHistory.Add(assistantMessage);
            if (model.ChatHistory.Count > 20)
                model.ChatHistory = model.ChatHistory
                                     .Skip(model.ChatHistory.Count - 20)
                                     .ToList();

            // Reload the list of recent sessions
            model.PreviousChats = _db.PreviousChats
                .Where(pc => pc.UserIdentityId == userId)
                .OrderByDescending(pc => pc.Date)
                .Take(10)
                .ToList();  // you can project if you need only Title/Date

            return View(model);
        }

       
        private async Task<string> GetOpenAiResponseStreamed(string userInput, ApplicationUser appUser)
        {
            var client = _httpClientFactory.CreateClient("OpenAI");

            var specs = (appUser?.Specializations ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var systemMessage = "";

            if (specs.Contains("Data Analytics"))
            {

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
                model = "gpt-4", 
                messages = new[]
     {
        new { role = "system", content = systemMessage },
        new { role = "user", content = userInput }
    },
                max_tokens = 1000, 
                temperature = 0.3, 
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
