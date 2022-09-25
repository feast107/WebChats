using Newtonsoft.Json;

namespace MonitorHub.Events
{
    public class Event<TS, TP, TO> : IEventArgs<TS, TP, TO>
    {
        [JsonProperty("s")]
        public TS? S { get; set; }

        [JsonProperty("p")]
        public TP? P { get; set; }

        [JsonProperty("o")]
        public TO? O { get; set; }

        [JsonProperty("v")]
        public string? V { get; set; }
    }
}
