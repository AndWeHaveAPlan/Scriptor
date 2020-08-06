using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AndWeHaveAPlan.Scriptor.AspExtensions
{
    public class ScriptorOptions
    {
        internal bool Json;
        internal JsonSerializerSettings JsonSerializerSettings;

        internal LoggerSettings LoggerSettings;

        internal bool OnlyScriptor;
        internal bool ExtendedErrorResponses;
        internal LogLevel MinLogLevel = LogLevel.Information;
        internal LogLevel AspMinLogLevel = LogLevel.Warning;
        internal bool SetMinimumLogLevel;
        internal bool RfcLogLevelNumbers;

        internal (string propertyName, string headerName)[] InjectedHeaders;

        /// <summary>
        /// Use plain json single line log messages
        /// </summary>
        /// <returns></returns>
        public ScriptorOptions UseJson(JsonLoggerOptions options = null)
        {
            Json = true;
            JsonSerializerSettings = options?.JsonSerializerSettings;
            return this;
        }

        public ScriptorOptions UseSettings(LoggerSettings loggerSettings = null)
        {
            LoggerSettings = loggerSettings ?? LoggerSettings.Default;
            return this;
        }

        /// <summary>
        /// Obsolete, use
        /// ConfigureLogging(builder => {
        ///     builder.SetMinimumLevel(...)
        ///     and
        ///     builder.AddFilter(...)
        /// }
        /// instead
        /// </summary>
        /// <param name="minLevel"></param>
        /// <param name="aspMinLevel"></param>
        /// <returns></returns>
        [Obsolete]
        public ScriptorOptions UseMinimumLogLevel(LogLevel minLevel, LogLevel aspMinLevel = LogLevel.Warning)
        {
            SetMinimumLogLevel = true;
            MinLogLevel = minLevel;
            AspMinLogLevel = aspMinLevel;
            return this;
        }

        /// <summary>
        /// Call Builder.SuppressStatusMessages(true) and .ConfigureLogging(logBuilder => logBuilder.ClearProviders();)
        /// </summary>
        /// <returns></returns>
        public ScriptorOptions UseOnlyScriptor()
        {
            OnlyScriptor = true;
            return this;
        }

        /// <summary>
        /// Obsolete, use
        /// IApplicationBuilder.UseHeadersAsScope 
        /// instead (like in Startup.Configure
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        [Obsolete("Use IApplicationBuilder.UseHeadersAsScope (like in Startup.Configure)", false)]
        public ScriptorOptions InjectHttpHeaders(params (string propertyName, string headerName)[] headers)
        {
            InjectedHeaders = headers;
            return this;
        }

        /// <summary>
        /// Unused by now
        /// </summary>
        /// <returns></returns>
        private ScriptorOptions UseExtendedErrorResponses()
        {
            ExtendedErrorResponses = true;
            return this;
        }

        /// <summary>
        /// Unused by now
        /// </summary>
        /// <returns></returns>
        private ScriptorOptions UseRfcLogLevelNumbers()
        {
            RfcLogLevelNumbers = true;
            return this;
        }
    }
}

