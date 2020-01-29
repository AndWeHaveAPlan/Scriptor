using System.Collections.Generic;
using System.Linq;
using AndWeHaveAPlan.Scriptor.Loggers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AndWeHaveAPlan.Scriptor.AspExtensions.Providers
{
    public class ScriptorLoggerProvider : ILoggerProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ScriptorOptions _options;
        private readonly bool _useJson;
        private readonly (string key, string value)[] _headers;

        public void Dispose()
        {
        }

        public ScriptorLoggerProvider(IHttpContextAccessor httpContextAccessor, ScriptorOptions options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _useJson = options.Json;

            if (_useJson)
            {
                JsonConsoleLogger.JsonSerializer = JsonSerializer.Create(
                    options.JsonSerializerSettings ??
                    new JsonSerializerSettings());
            }

            _headers = options.InjectedHeaders ?? new (string key, string value)[0];
        }

        public ILogger CreateLogger(string categoryName)
        {
            ScriptorLogger logger;

            if (_useJson)
            {
                logger = new JsonConsoleLogger(categoryName, true);
            }
            else
            {
                logger = new ColoredConsoleLogger(categoryName, true);
            }

            logger.LoggerSettings = _options?.LoggerSettings ?? LoggerSettings.Default;

            logger.InjectData(InjectFunc);

            logger.UseRfcLogLevel();

            return logger;
        }

        private Dictionary<string, string> InjectFunc()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var (key, value) in _headers)
            {
                result.Add(key, _httpContextAccessor.HttpContext?.Request.Headers[value].FirstOrDefault());
            }

            return result;
        }
    }
}