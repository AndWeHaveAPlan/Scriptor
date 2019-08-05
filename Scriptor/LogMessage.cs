using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Scriptor
{
    public class LogMessage
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
        public DateTime Timestamp;
    }
}
