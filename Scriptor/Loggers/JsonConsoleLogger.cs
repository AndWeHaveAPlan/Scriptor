using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scriptor.Loggers
{
    public class JsonConsoleLogger : ScriptorLogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeScopes"></param>
        public JsonConsoleLogger(string name, bool includeScopes = false) : base(name, includeScopes)
        {

        }

        protected override List<QueueItem> ComposeInternal(LogMessage message)
        {
            var result = new List<QueueItem>();

            var jObject = JObject.FromObject(message);

            if (message.AuxData != null)
            {
                foreach (var (key, value) in message.AuxData)
                {
                    jObject.Add(jObject.ContainsKey(key) ? $"aux_{key}" : key, value);
                }
            }

            result.Add(new QueueItem
            {
                String = jObject.ToString(Formatting.None)
            });

            return result;
        }
    }
}