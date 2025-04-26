using OpenAI_API;

public class OpenAIService
{
    private readonly OpenAIAPI _api;

    public OpenAIService()
    {
        _api = new OpenAIAPI("sk-proj-TQTlyjvM1QCZskEB4sCtT3BlbkFJmJxQLcOt1pOBPTpX9W1X");
    }

    public async Task<string> AskAsync(string prompt)
    {
        var chat = _api.Chat.CreateConversation();
        chat.AppendUserInput(prompt);
        return await chat.GetResponseFromChatbotAsync();
    }
}
