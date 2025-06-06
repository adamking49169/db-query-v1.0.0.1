using Microsoft.Extensions.Configuration;
using OpenAI_API;

public class OpenAIService
{
    private readonly OpenAIAPI _api;

    public OpenAIService(IConfiguration configuration)
    {
        var apiKey = configuration["OPENAI_API_KEY"] ??
                     Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not configured.");
        }
        _api = new OpenAIAPI(apiKey);
    }

    public async Task<string> AskAsync(string prompt)
    {
        var chat = _api.Chat.CreateConversation();
        chat.AppendUserInput(prompt);
        return await chat.GetResponseFromChatbotAsync();
    }
}
