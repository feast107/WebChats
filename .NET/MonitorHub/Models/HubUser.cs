using Newtonsoft.Json;
namespace MonitorHub.Models
{
    public class HubUser 
    {
        public HubUser() { }
        public HubUser(bool IsServer)
        {
            SERVER_DATA = IsServer;
        }

        [JsonProperty("userId")]
        public string ?UserId { get; set; }

        [JsonProperty("userName")]
        public string ?UserName { get; set; }
        
        [JsonProperty("connectionId")]
        public string ?ConnectionId { get; set; }

        [JsonIgnore]
        public List<HubGroup> ?HubGroups { get; set; }

        [JsonIgnore]
        public HubGroup? Own { get; set; }

        [JsonProperty("groups")]
        public List<string?>? Groups {
            get {
                if (SERVER_DATA)
                {
                    return HubGroups?.Select(x => x.GroupId).ToList();
                }
                else
                {
                    return groups;
                }
            }
            set { groups = value; }
        }

        private List<string?>? groups;

        private readonly bool SERVER_DATA = false;
    }
}
