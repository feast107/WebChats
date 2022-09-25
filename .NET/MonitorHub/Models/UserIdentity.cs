using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MonitorHub.Models
{
    
    public class UserIdentity
    {
        [DisplayName("连接标识符")]
        [JsonProperty("connectionId")]
        public string ?ConnectionId { get; set; }

        [DisplayName("用户编号")]
        [JsonProperty("userId")]
        public string ?UserId { get; set; }

        [DisplayName("用户名")]
        [Required]
        [JsonProperty("userName")]
        public string? UserName { get; set; }

        [DisplayName("密码")]
        [JsonProperty("password")]
        public string? Password { get; set; }

        [DisplayName("身份")]
        [Required]
        [JsonProperty("role")]
        public string? Role { get; set; }
    }
}
