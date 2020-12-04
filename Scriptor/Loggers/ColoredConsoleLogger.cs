using AndWeHaveAPlan.Scriptor.Processing;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    public class ColoredConsoleLogger : ColoredLogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scopeProvider"></param>
        public ColoredConsoleLogger(string name, IExternalScopeProvider scopeProvider) : base(name, scopeProvider, new ConsoleLogProcessor())
        {
        }
    }
}