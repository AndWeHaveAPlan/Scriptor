## Scriptor
#### Example
```
public class CustomLoggerProvider : ILoggerProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomLoggerProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            ScriptorLogger logger = new ColoredConsoleLogger(categoryName, true);

            logger.InjectData(() =>
            {
                // Additional data included in log messages
                return new Dictionary<string, string>
                {
                    { "cf-ray-id" , _httpContextAccessor.HttpContext?.Request.Headers["CF-RAY"].FirstOrDefault() }
                };
            });

            // Custom formatting method
            logger.UseComposer(message =>
            {
                var result = new List<QueueItem>();

                result.Add(
                    new QueueItem { ForegroundColor = ConsoleColor.Green, String = message.LevelString }
                );

                foreach (var (key, value) in message.AuxData)
                {
                    result.Add(
                        new QueueItem { ForegroundColor = ConsoleColor.Yellow, String = $"{key}: {value}" }
                    );
                }

                return result;
            });

            // Use Syslog RFC severity numbers
            // instead of Microsoft.Extensions.Logging.LogLevel values
            // RFC 5424 https://www.rfc-editor.org/rfc/rfc5424.txt [Page 10]
            logger.UseRfcLogLevel();

            return logger;
        }
        public void Dispose()
        {
        }
    }
```