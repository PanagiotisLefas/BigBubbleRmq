using System;
using System.Text.Json.Serialization;

namespace BigBubble.Models
{
    public class JoinModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "join";

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public class LeaveModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "leave";
        
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public class MessageModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "publish";

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class MessageReceivedModelBase
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
        public long Received => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public class MessageReceivedModel : MessageReceivedModelBase
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

    }

    public static class Helpers
    {
        public static MessageReceivedModel Sanitize(this MessageReceivedModel message)
        {
            var sanitized = new MessageReceivedModel()
            {
                Message = message?.Message?.TrimString(700),
                Nickname = message?.Nickname?.TrimString(30),
                Type = message?.Type,
            };
            return sanitized;
        }

        public static string TrimString(this string str, int max)
        {
            if (str.Length > max)
            {
                str = str.Substring(0, max - 1) + "...";
            }

            return str;
        }
    }

    public enum KnownTypes
    {
        join = 1, leave = 2, publish = 3
    }
}
