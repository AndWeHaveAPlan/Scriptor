using System;
using System.Collections.Generic;
using AndWeHaveAPlan.Scriptor.Loggers;
using Microsoft.Extensions.Logging;

namespace Scriptor.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonConsoleLogger logger = new JsonConsoleLogger("rest logger", true);

            logger.InjectData(() => new Dictionary<string, string>
            {
                {"RequestId", "GDSGSD" },
                {"RayId", "EGDS4GES5" },
            });

            logger.LogInformation("test message");
            logger.LogInformation("test message2");
            logger.LogInformation("test message3");

            Console.ReadLine();
        }
    }
}
