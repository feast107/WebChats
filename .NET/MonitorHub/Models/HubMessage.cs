using Newtonsoft.Json;
namespace MonitorHub.Models
{
    public class HubMessage
    {
        [JsonProperty("content")]
        public string? Content { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        public enum MessageType
        {
            chars,
            picture
        }
    }
}
