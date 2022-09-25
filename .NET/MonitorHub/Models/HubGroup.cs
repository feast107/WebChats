using Newtonsoft.Json;
namespace MonitorHub.Models
{
    public class HubGroup
    {
        [JsonProperty("creator")]
        public HubUser? Creator { get; set; }

        [JsonProperty("groupId")]
        public string? GroupId { get; set; }

        [JsonProperty("groupName")]
        public string? GroupName { get; set; }

        [JsonProperty("clients")]
        public List<HubUser>? Clients { get; set; }
    }

}
