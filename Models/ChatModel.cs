using System;
using System.Collections.Generic;

namespace Models
{
    public class ChatModel
    {
        public string UserInput { get; set; }

        public List<ChatHistoryItem> ChatHistory { get; set; } = new();

        public List<DataRow> Data { get; set; } = new();
        public bool PasscodeValidated { get; set; }

        public bool UsingDummyData { get; set; }

        public string Passcode { get; internal set; }

        public List<PreviousChat> PreviousChats { get; set; } = new();
    }

    public class ChatHistoryItem
    {
        public int Id { get; set; }
        public string UserIdentityId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DataRow
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
    }

    public class PreviousChat
    {
        public int Id { get; set; }
        public string UserIdentityId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
    }
}
