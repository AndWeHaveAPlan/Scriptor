using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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

        private IExternalScopeProvider _scopeProvider= new LoggerExternalScopeProvider();

        public void Dispose()
        {
        }

        public ScriptorLoggerProvider(IHttpContextAccessor httpContextAccessor, ScriptorOptions options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options;
            _useJson = options?.Json == true;

            if (_useJson)
            {
                JsonConsoleLogger.JsonSerializer = JsonSerializer.Create(
                    options?.JsonSerializerSettings ??
                    new JsonSerializerSettings());
            }

            _headers = options?.InjectedHeaders ?? new (string key, string value)[0];
        }

        public ILogger CreateLogger(string categoryName)
        {
            ScriptorLogger logger;

            var scopeProvider = _scopeProvider ;//?? new LoggerExternalScopeProvider();

            if (_useJson)
            {
                logger = new JsonConsoleLogger(categoryName, scopeProvider);
            }
            else
            {
                logger = new ColoredConsoleLogger(categoryName, scopeProvider);
            }

            logger.LoggerSettings = _options?.LoggerSettings ?? LoggerSettings.Default;

            logger.InjectData(InjectFunc);

            if (_options?.RfcLogLevelNumbers == true)
                logger.UseRfcLogLevel();

            return logger;
        }

        private Dictionary<string, string> InjectFunc(LogLevel logLevel)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var (key, value) in _headers)
            {
                result.Add(key, _httpContextAccessor.HttpContext?.Request.Headers[value].FirstOrDefault());
            }


            if (logLevel <= LogLevel.Debug)
            {
                var process = Process.GetCurrentProcess();
                var thread = Thread.CurrentThread;

                result.Add("net_process_id", process.Id.ToString());
                result.Add("net_thread_id", thread.ManagedThreadId.ToString());
            }

            return result;
        }

        public ScriptorLoggerProvider UseScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
            return this;
        }
    }
}