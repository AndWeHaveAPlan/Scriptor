using System.Collections.Generic;
using Newtonsoft.Json;

namespace AndWeHaveAPlan.Scriptor
{
    public struct LogMessage
    {
        [JsonIgnore]
        public Dictionary<string, string> AuxData;

        [JsonProperty("scope")]
        public string Scope;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("exception")]
        public string Exception;

        [JsonProperty("level_string")]
        public string LevelString;

        [JsonProperty("level")]
        public int Level;

        [JsonProperty("timestamp")]
        public string Timestamp;

        [JsonProperty("log_name")]
        public string LogName;

        [JsonProperty("event_id")]
        public string EventId;
    }
}
