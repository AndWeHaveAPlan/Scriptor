using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using AndWeHaveAPlan.Scriptor.Scopes;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    /// <summary>
    /// Base class for scriptor loggers
    /// </summary>
    public abstract class ScriptorLogger : ILogger, ISupportExternalScope
    {
        private static readonly ConsoleLogProcessor QueueProcessor = new ConsoleLogProcessor();
        private Func<string, LogLevel, bool> _filter;

        public LoggerSettings LoggerSettings = LoggerSettings.Default;

        private IExternalScopeProvider _scopeProvider;

        private static readonly List<Regex> FieldRegex = new List<Regex>
        {
            new Regex(@"\{\{\s*([a-zA-Z_][\w-_]*)\s*:\s*(.*[^\s])\s*\}\}", RegexOptions.Compiled),
            new Regex(@"\[\s*([a-zA-Z_][\w-_]*)\s*:\s*(.*[^\s])\s*\]", RegexOptions.Compiled)
        };

        protected bool UseRfcLevel;

        /// <summary>
        /// 
        /// </summary>
        protected Func<LogLevel, Dictionary<string, string>> Inject;

        /// <summary>
        /// 
        /// </summary>
        protected Func<LogMessage, List<QueueItem>> Compose;

        protected ScriptorLogger(string name, IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Filter = (category, logLevel) => true;

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
        public bool IncludeScopes => true;

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
                WriteMessage(logLevel, eventId.Id, message, exception);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        private void WriteMessage(LogLevel logLevel, int eventId, string message, Exception exception)
        {
            var logMessage = new LogMessage
            {
                Timestamp = DateTime.UtcNow.ToString(LoggerSettings.TimestampFormat),
                LevelString = GetLogLevelString(logLevel),
                Level = UseRfcLevel ? GetLogLevelRfcNumber(logLevel) : (int)logLevel,
                Message = message,
                Exception = exception?.ToString(),
                AuxData = Inject?.Invoke(logLevel) ?? new Dictionary<string, string>(),
                EventId = eventId.ToString(),
                LogName = Name
            };

            var scopeFields = ExtractFieldFromScope(_scopeProvider);
            logMessage = AddAuxData(logMessage, scopeFields);

            var messageFields = ExtractFieldFromMessage(logMessage);
            logMessage = AddAuxData(logMessage, messageFields);

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
        public void InjectData(Func<LogLevel, Dictionary<string, string>> injectFunc)
        {
            Inject = injectFunc;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Filter?.Invoke(Name, logLevel) ?? true;
        }

        /// <summary>
        /// Begin logging scope
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            var scope = _scopeProvider?.Push(state);
            return scope;
        }

        private string GetScopeInformation()
        {
            var stringBuilder = new StringBuilder();

            _scopeProvider?.ForEachScope((scopeObj, state) =>
            {
                if (state.Length != 0)
                    state.Append("\r\n");
                state.Append(scopeObj);
            }, stringBuilder);


            return stringBuilder.ToString();
        }

        private static Dictionary<string, object> ExtractFieldFromMessage(LogMessage logMessage)
        {
            List<Match> matches = new List<Match>();

            var resultDictionary = new Dictionary<string, object>();

            foreach (var regex in FieldRegex)
            {
                matches.AddRange(regex.Matches(logMessage.Message));
            }

            //if (matches.Count > 0 && logMessage.AuxData == null)
            //logMessage.AuxData = new Dictionary<string, string>();

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                //logMessage = AddAuxData(logMessage, key, value);
                resultDictionary[key] = value;
            }

            return resultDictionary;
        }

        public static Dictionary<string, object> ExtractFieldFromScope(IExternalScopeProvider scopeProvider)
        {
            var result = new Dictionary<string, object>();

            scopeProvider.ForEachScope((scopeObj, resultDictionary) =>
            {
                switch (scopeObj)
                {
                    case ParameterizedLogScopeItem scopeItem:
                        resultDictionary[scopeItem.Key] = scopeItem.Value;
                        break;
                    case ParameterizedLogScope scope:
                        foreach (var (key, value) in scope)
                            resultDictionary[key] = value;
                        break;

                }
            }, result);

            return result;
        }

        private static LogMessage AddAuxData(LogMessage logMessage, string key, object value)
        {
            if (logMessage.AuxData.ContainsKey(key))
                logMessage.AuxData[key] = value.ToString();
            else
                logMessage.AuxData.Add(key, value.ToString());

            return logMessage;
        }

        private static LogMessage AddAuxData(LogMessage logMessage, Dictionary<string, object> fields)
        {
            foreach (var (key, value) in fields)
            {
                logMessage = AddAuxData(logMessage, key, value);
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

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}