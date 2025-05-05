using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using db_query_v1._0._0._1.Models;   

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

        // FK to AspNetUsers
        public string UserIdentityId { get; set; }
        public ApplicationUser User { get; set; }

        // FK to PreviousChat
        public int ChatId { get; set; }
        public PreviousChat PreviousChat { get; set; }

        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ImageUrl { get; set; }
    }

    public class DataRow
    {
        public string ColumnName { get; set; }
        public string Value { get; set; }
    }

    public class PreviousChat
    {
        [Key]
        public int Id { get; set; }
        // FK to AspNetUsers
        public string UserIdentityId { get; set; }
        public ApplicationUser User { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public ICollection<ChatHistoryItem> ChatHistoryItems { get; set; } = new List<ChatHistoryItem>();
    }
}
