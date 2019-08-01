using System;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Scriptor
{
    public class ColoredConsoleLogger : ILogger
    {
        private readonly string _requestId;

        private readonly ConsoleLogProcessor _queueProcessor;
        private Func<string, LogLevel, bool> _filter;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        protected virtual string DateTimeFormat => "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// 
        /// </summary>
        static ColoredConsoleLogger()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeScopes"></param>
        /// <param name="requestId"></param>
        public ColoredConsoleLogger(string name, bool includeScopes, string requestId)
        {
            _requestId = requestId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Filter = ((category, logLevel) => true);
            IncludeScopes = includeScopes;

            _queueProcessor = new ConsoleLogProcessor();
        }

        /// <summary>
        /// 
        /// </summary>
        public Func<string, LogLevel, bool> Filter
        {
            get => _filter;
            set => _filter = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, Name, eventId.Id, message, exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logName"></param>
        /// <param name="eventId"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logBuilder = _logBuilder;
            _logBuilder = null;

            var logMessage = new LogMessage();

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            var logLevelString = string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                logLevelString = GetLogLevelString(logLevel);

                logMessage.Header = _requestId == null
                    ? $"[ {logLevelString} | {DateTime.UtcNow.ToString(DateTimeFormat)}]"
                    : $"[ {logLevelString} | {DateTime.UtcNow.ToString(DateTimeFormat)} | {_requestId} ]";

                if (IncludeScopes)
                    logMessage.Scope = GetScopeInformation();

                logMessage.Message = message;
            }

            if (exception != null)
            {
                // exception message
                logMessage.Exception = exception.ToString();
            }

            var hasLevel = !string.IsNullOrEmpty(logLevelString);
            // Queue log message
            _queueProcessor.EnqueueMessage(logMessage);


            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter(Name, logLevel);
        }


        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return ConsoleLogScope.Push(Name, state);
        }

        private string GetScopeInformation()
        {
            var stringBuilder = new StringBuilder();

            var current = ConsoleLogScope.Current;

            while (current != null)
            {
                stringBuilder.Append("=> ");
                stringBuilder.Append(current);
                current = current.Parent;
            }

            return stringBuilder.ToString();
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "TRACE";
                case LogLevel.Debug:
                    return "DEBUG";
                case LogLevel.Information:
                    return "INFO";
                case LogLevel.Warning:
                    return "WARN";
                case LogLevel.Error:
                    return "ERROR";
                case LogLevel.Critical:
                    return "CRITICAL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}