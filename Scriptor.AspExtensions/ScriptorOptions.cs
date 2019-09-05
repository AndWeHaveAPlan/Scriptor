using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.AspExtensions
{
    public class ScriptorOptions
    {
        internal bool Json;
        internal bool OnlyScriptor;
        internal bool ExtendedErrorResponses;
        internal LogLevel MinLogLevel = LogLevel.Information;
        internal LogLevel AspMinLogLevel = LogLevel.Warning;

        internal (string propertyName, string headerName)[] InjectedHeaders;

        /// <summary>
        /// Use plain json single line log messages
        /// </summary>
        /// <returns></returns>
        public ScriptorOptions UseJson()
        {
            Json = true;
            return this;
        }

        public ScriptorOptions UseMinimumLogLevel(LogLevel minLevel, LogLevel aspMinLevel = LogLevel.Warning)
        {
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
    }
}

