using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AndWeHaveAPlan.Scriptor.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AndWeHaveAPlan.Scriptor.Test
{
    [TestClass]
    public class ConsoleLogProcessorTest
    {
        private List<string> _messages = new List<string>
        {
            "first message",
            "second message",
            "third message"
        };

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public async Task SimpleWrite()
        {
            var processor = new ConsoleLogProcessor();
            var defaultConsoleOut = Console.Out;

            await using var stream = new MemoryStream();

            await using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                Console.SetOut(writer);

                foreach (var message in _messages)
                {
                    processor.EnqueueMessage(message);
                }

                var complexMessage = CreateQueueItems("test 1");

                processor.EnqueueMessage(complexMessage);

                await Task.Delay(1000);
            }

            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);

            stream.Position = 0;

            List<string> log = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    log.Add(await reader.ReadLineAsync());
                }
            }

            Assert.AreEqual(log[0], "first message");
            Assert.AreEqual(log[1], "second message");
            Assert.AreEqual(log[2], "third message");
            Assert.AreEqual(log[3], "test 1 first part");
            Assert.AreEqual(log[4], "test 1 second part");
            Assert.AreEqual(log[5], "test 1 third part");
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public async Task ConcurrentWriteWrite()
        {
            var processor = new ConsoleLogProcessor();
            var defaultConsoleOut = Console.Out;

            await using var stream = new MemoryStream();

            await using (var writer = new StreamWriter(stream, leaveOpen: true))
            {
                Console.SetOut(writer);

                List<Task> tasks = new List<Task>();

                var letters = "ABCDEFGHJKLMNOP";

                for (int i = 0; i < 10; i++)
                {
                    var i1 = i;
                    var task = Task.Factory.StartNew(() =>
                    {
                        var message = CreateQueueItems(letters[i1].ToString(), 20);
                        processor.EnqueueMessage(message);
                    });

                    tasks.Add(task);
                }

                await Task.Delay(1000);
            }

            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);

            stream.Position = 0;

            List<string> log = new List<string>();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    log.Add(await reader.ReadLineAsync());
                }
            }

            for (int m = 0; m < 10; m++)
            {
                var letter = log[m*21][0];

                for (int l = 0; l < 20; l++)
                {
                    var index = m * 21 + l;
                    Assert.AreEqual(log[index], $"{letter} {l+1}'st part");
                }
            }
        }

        private IEnumerable<QueueItem> CreateQueueItems(string prefix)
        {
            return new List<QueueItem>
            {
                new QueueItem{String = $"{prefix} first part\n"},
                new QueueItem{String = $"{prefix} second part\n"},
                new QueueItem{String = $"{prefix} third part"}
            };
        }

        private IEnumerable<QueueItem> CreateQueueItems(string prefix, int count)
        {
            var result = new List<QueueItem>();

            for (int i = 0; i < count; i++)
            {
                result.Add(new QueueItem { String = $"{prefix} {i + 1}'st part\n" });
            }

            return result;
        }
    }
}
