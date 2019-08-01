using Microsoft.Extensions.Logging;

namespace Scriptor
{
    internal class LogMessage
    {
        public string Header;
        public string Scope;
        public string Message;
        public string Exception;
        public LogLevel LogLevel;
    }
}
