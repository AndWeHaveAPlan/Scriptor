using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Scriptor.Loggers;

namespace Scriptor.AspExtensions.Providers
{
    public class ScriptorLoggerProvider : ILoggerProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly bool _useJson;
        private readonly (string key, string value)[] _headers;

        public void Dispose()
        {
        }

        public ScriptorLoggerProvider(IHttpContextAccessor httpContextAccessor, bool useJson = false, params (string key, string value)[] injectHeaders)
        {
            _httpContextAccessor = httpContextAccessor;
            _useJson = useJson;
            _headers = injectHeaders;
        }

        public ILogger CreateLogger(string categoryName)
        {
            ScriptorLogger logger;

            if (_useJson)
                logger = new JsonConsoleLogger(categoryName, true);
            else
                logger = new ColoredConsoleLogger(categoryName, true);

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