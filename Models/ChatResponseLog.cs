using System;

namespace db_query_v1._0._0._1.Models
{
    public class ChatResponseLog
    {
        public int Id { get; set; }           
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
