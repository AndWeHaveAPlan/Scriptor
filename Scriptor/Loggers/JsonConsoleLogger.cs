using System.Collections.Generic;
using AndWeHaveAPlan.Scriptor.Processing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    public class JsonConsoleLogger : JsonLogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scopeProvider"></param>
        public JsonConsoleLogger(string name, IExternalScopeProvider scopeProvider) : base(name, scopeProvider, ConsoleLogProcessor.GetDefault)
        {
        }
    }
}