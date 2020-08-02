using System;
using AndWeHaveAPlan.Scriptor.Scopes;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor
{
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        public static IDisposable BeginParamScope(this ILogger logger, (string, object) item)
        {
            return logger.BeginScope(new ParameterizedLogScopeItem(item));
        }

        public static IDisposable BeginParamScope(this ILogger logger, params (string, object)[] items)
        {
            return logger.BeginScope(new ParameterizedLogScope(items));
        }
    }
}
