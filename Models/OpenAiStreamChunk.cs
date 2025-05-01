// Models/OpenAiStreamChunk.cs

// Models/OpenAiStreamChunk.cs
using Newtonsoft.Json;

namespace db_query_v1._0._0._1.Models
{
    public class OpenAiStreamChunk
    {
        [JsonProperty("choices")]
        public Choice[] Choices { get; set; }
    }

    public class Choice
    {
        [JsonProperty("delta")]
        public Delta Delta { get; set; }

        // The API also emits `index` and `finish_reason`, so you can include them if you need:
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }

    public class Delta
    {
        // In early chunks you'll often get only a `role` token,
        // but when content starts streaming you'll get `content`.
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
