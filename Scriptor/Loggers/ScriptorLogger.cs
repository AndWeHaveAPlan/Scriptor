using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    /// <summary>
    /// Base class for scriptor loggers
    /// </summary>
    public abstract class ScriptorLogger : ILogger
    {
        private static readonly ConsoleLogProcessor QueueProcessor = new ConsoleLogProcessor();
        private Func<string, LogLevel, bool> _filter;

        public LoggerSettings LoggerSettings = LoggerSettings.Default;

        private static readonly List<Regex> FieldRegex = new List<Regex>
        {
            new Regex(@"\{\{\s*([\w-]*):\s*([\w\s]*\w)\s*\}\}"),
            new Regex(@"\[\s*([\w-]*):\s*([\w\s]*\w)\s*\]")
        };

        protected bool UseRfcLevel;

        /// <summary>
        /// 
        /// </summary>
        protected Func<Dictionary<string, string>> Inject;

        /// <summary>
        /// 
        /// </summary>
        protected Func<LogMessage, List<QueueItem>> Compose;

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
        private void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logMessage = new LogMessage
            {
                Timestamp = DateTime.UtcNow.ToString(LoggerSettings.TimestampFormat),
                LevelString = GetLogLevelString(logLevel),
                Level = UseRfcLevel ? GetLogLevelRfcNumber(logLevel) : (int)logLevel,
                Message = message,
                Exception = exception?.ToString(),
                AuxData = Inject?.Invoke()
            };

            logMessage = ExtractField(logMessage);

            if (IncludeScopes)
                logMessage.Scope = GetScopeInformation();

            var queueItems = Compose(logMessage);

            QueueProcessor.EnqueueMessage(queueItems);
        }

        /// <summary>
        /// Default compose func
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract List<QueueItem> ComposeInternal(LogMessage message);

        /// <summary>
        /// Custom logMessage formatting method (executed after custom data injection)
        /// </summary>
        /// <param name="composeFunc"></param>
        public void UseComposer(Func<LogMessage, List<QueueItem>> composeFunc)
        {
            Compose = composeFunc;
        }

        /// <summary>
        /// Use Syslog RFC severity numbers instead of Microsoft.Extensions.Logging.LogLevel values
        /// RFC 5424 https://www.rfc-editor.org/rfc/rfc5424.txt [Page 10]
        /// </summary>
        public void UseRfcLogLevel()
        {
            UseRfcLevel = true;
        }

        /// <summary>
        /// Inject custom data in LogMessage.AuxData (executed before composing logMessage)
        /// </summary>
        /// <param name="injectFunc"></param>
        public void InjectData(Func<Dictionary<string, string>> injectFunc)
        {
            Inject = injectFunc;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter?.Invoke(Name, logLevel) ?? true;
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

        private static LogMessage ExtractField(LogMessage logMessage)
        {
            List<Match> matches = new List<Match>();

            foreach (var regex in FieldRegex)
            {
                matches.AddRange(regex.Matches(logMessage.Message));
            }

            if (matches.Count > 0 && logMessage.AuxData == null)
                logMessage.AuxData = new Dictionary<string, string>();

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                if (logMessage.AuxData.ContainsKey(key))
                {
                    logMessage.AuxData[key] = value;
                }
                else
                {
                    logMessage.AuxData.Add(key, value);
                }
            }

            return logMessage;
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