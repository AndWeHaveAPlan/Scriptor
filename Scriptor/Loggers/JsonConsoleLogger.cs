using System.Collections.Generic;
using AndWeHaveAPlan.Scriptor.Processing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    public class JsonConsoleLogger : ScriptorLogger
    {
        public static JsonSerializer JsonSerializer = JsonSerializer.CreateDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scopeProvider"></param>
        public JsonConsoleLogger(string name, IExternalScopeProvider scopeProvider) : base(name,  scopeProvider)
        {
        }

        protected override List<QueueItem> ComposeInternal(LogMessage message)
        {
            var result = new List<QueueItem>();

            var jObject = JObject.FromObject(message, JsonSerializer);

            if (message.AuxData != null)
            {
                foreach (var (key, value) in message.AuxData)
                {
                    jObject.Add(jObject.ContainsKey(key) ? $"aux_{key}" : key, value);
                }
            }

            result.Add(new QueueItem
            {
                String = jObject.ToString(Formatting.Indented)
            });

            return result;
        }
    }
}