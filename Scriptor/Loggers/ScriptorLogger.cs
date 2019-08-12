using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    public abstract class ScriptorLogger : ILogger
    {
        private static readonly ConsoleLogProcessor QueueProcessor;
        private Func<string, LogLevel, bool> _filter;

        protected bool UseRfcLevel;

        /// <summary>
        /// 
        /// </summary>
        protected Func<Dictionary<string, string>> Inject;

        /// <summary>
        /// 
        /// </summary>
        protected Func<LogMessage, QueueItem[]> Compose;

        static ScriptorLogger()
        {
            QueueProcessor = new ConsoleLogProcessor();
        }

        protected ScriptorLogger(string name, bool includeScopes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Filter = (category, logLevel) => true;
            IncludeScopes = includeScopes;

            Compose = ComposeInternal;
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
            var logMessage = new LogMessage
            {
                Timestamp = DateTime.UtcNow,
                LevelString = GetLogLevelString(logLevel),
                Level = UseRfcLevel ? GetLogLevelRfcNumber(logLevel) : (int)logLevel,
                Message = message,
                Exception = exception?.ToString(),
                AuxData = Inject?.Invoke()
            };

            if (IncludeScopes)
                logMessage.Scope = GetScopeInformation();

            var queueItem = Compose(logMessage);

            QueueProcessor.EnqueueMessage(queueItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract QueueItem[] ComposeInternal(LogMessage message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="composeFunc"></param>
        public void UseComposer(Func<LogMessage, QueueItem[]> composeFunc)
        {
            Compose = composeFunc;
        }

        public void UseRfcLogLevel()
        {
            UseRfcLevel = true;
        }

        public void InjectData(Func<Dictionary<string, string>> injectFunc)
        {
            Inject = injectFunc;
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
                stringBuilder.Append("=>");
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

        private static int GetLogLevelRfcNumber(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return 7;
                case LogLevel.Debug:
                    return 7;
                case LogLevel.Information:
                    return 6;
                case LogLevel.Warning:
                    return 4;
                case LogLevel.Error:
                    return 3;
                case LogLevel.Critical:
                    return 2;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}