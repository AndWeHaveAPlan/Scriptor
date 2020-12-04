using System;
using System.Linq;
using AndWeHaveAPlan.Scriptor.Loggers;
using AndWeHaveAPlan.Scriptor.Test.Stuff;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndWeHaveAPlan.Scriptor.Test
{
    [TestClass]
    public class ColoredLoggerTest
    {

        [TestMethod]
        public void Bitwise()
        {
            var a = 22;
            var b = 30;

            if (a < 29 ^ b < 29)
            {

            }
        }


        [TestMethod]
        public void SimpleWrite()
        {
            var logProcessor = new TestLogProcessor();
            ILogger logger = new ColoredLogger("Test logger", new LoggerExternalScopeProvider(), logProcessor);

            logger.LogInformation("Sample message");

            var firstLogEntry = logProcessor.Log.FirstOrDefault();

            Assert.IsNotNull(firstLogEntry, "Log not passed to ILogProcessor");

            var logList = firstLogEntry.ToList();

            Assert.IsTrue(logList[0].String.StartsWith("[ INFO | 2 | "), "Incorrect 'header' start");
            Assert.IsNull(logList[0].BackgroundColor, "header BackgroundColor is not null");
            Assert.AreEqual(logList[0].ForegroundColor, ConsoleColor.Green, "header ForegroundColor color is not Green");
            Assert.IsTrue(logList[0].String.EndsWith("Test logger:\n"), "Incorrect 'header' end");

            Assert.AreEqual(logList[1].String, "Sample message", "Incorrect 'message'");

            Assert.AreEqual(logList[2].String, "\n", "Incorrect 'padding'");
        }

        [TestMethod]
        public void ParametrizedWrite()
        {
            var logProcessor = new TestLogProcessor();
            ILogger logger = new ColoredLogger("Test logger", new LoggerExternalScopeProvider(), logProcessor);

            logger.LogInformation("Sample message { testKey1: testVal1 } [ testKey2: testVal2 ]");

            var firstLogEntry = logProcessor.Log.FirstOrDefault();

            Assert.IsNotNull(firstLogEntry, "Log not passed to ILogProcessor");

            var logList = firstLogEntry.ToList();

            Assert.IsTrue(logList[0].String.StartsWith("[ INFO | 2 | "), "Incorrect 'header' start");
            Assert.IsNull(logList[0].BackgroundColor, "header BackgroundColor is not null");
            Assert.AreEqual(logList[0].ForegroundColor, ConsoleColor.Green, "header ForegroundColor color is not Green");
            Assert.IsTrue(logList[0].String.EndsWith("Test logger:\n"), "Incorrect 'header' end");

            Assert.AreEqual(logList[1].String, "testKey1: testVal1\ntestKey2: testVal2\n", "Incorrect 'params'");

            Assert.AreEqual(logList[2].String, "Sample message { testKey1: testVal1 } [ testKey2: testVal2 ]", "Incorrect 'message'");
            Assert.AreEqual(logList[3].String, "\n", "Incorrect 'padding'");
        }

        [TestMethod]
        public void DefaultScopeWrite()
        {
            var logProcessor = new TestLogProcessor();
            ILogger logger = new ColoredLogger("Test logger", new LoggerExternalScopeProvider(), logProcessor);

            using (logger.BeginScope("test scope0"))
            {
                using (logger.BeginScope("test scope1"))
                {
                    logger.LogInformation("Sample message");
                }
            }

            var firstLogEntry = logProcessor.Log.FirstOrDefault();

            Assert.IsNotNull(firstLogEntry, "Log not passed to ILogProcessor");

            var logList = firstLogEntry.ToList();

            Assert.IsTrue(logList[0].String.StartsWith("[ INFO | 2 | "), "Incorrect 'header' start");
            Assert.IsNull(logList[0].BackgroundColor, "header BackgroundColor is not null");
            Assert.AreEqual(logList[0].ForegroundColor, ConsoleColor.Green, "header ForegroundColor color is not Green");
            Assert.IsTrue(logList[0].String.EndsWith("Test logger:\n"), "Incorrect 'header' end");

            Assert.AreEqual(logList[1].String, "test scope0\ntest scope1", "Incorrect 'scope'");

            Assert.AreEqual(logList[3].String, "Sample message", "Incorrect 'message'");
            Assert.AreEqual(logList[4].String, "\n", "Incorrect 'padding'");
        }

        [TestMethod]
        public void ParametrizedScopeWrite()
        {
            var logProcessor = new TestLogProcessor();
            ILogger logger = new ColoredLogger("Test logger", new LoggerExternalScopeProvider(), logProcessor);

            using (logger.BeginParamScope(("testKey3", "testVal3"), ("testKey4", "testVal4")))
            {
                logger.LogInformation("Sample message");
            }

            var firstLogEntry = logProcessor.Log.FirstOrDefault();

            Assert.IsNotNull(firstLogEntry, "Log not passed to ILogProcessor");

            var logList = firstLogEntry.ToList();

            Assert.IsTrue(logList[0].String.StartsWith("[ INFO | 2 | "), "Incorrect 'header' start");
            Assert.IsNull(logList[0].BackgroundColor, "header BackgroundColor is not null");
            Assert.AreEqual(logList[0].ForegroundColor, ConsoleColor.Green, "header ForegroundColor color is not Green");
            Assert.IsTrue(logList[0].String.EndsWith("Test logger:\n"), "Incorrect 'header' end");

            Assert.AreEqual(logList[1].String, "testKey3: testVal3, testKey4: testVal4", "Incorrect 'scope'");

            Assert.AreEqual(logList[3].String, "testKey3: testVal3\ntestKey4: testVal4\n", "Incorrect 'params' from scope");

            Assert.AreEqual(logList[4].String, "Sample message", "Incorrect 'message'");
            Assert.AreEqual(logList[5].String, "\n", "Incorrect 'padding'");
        }
    }
}
