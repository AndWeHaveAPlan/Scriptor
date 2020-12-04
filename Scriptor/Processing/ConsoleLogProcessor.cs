using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Scriptor.Processing
{
    public sealed class ConsoleLogProcessor : IDisposable, ILogProcessor
    {
        private static readonly BlockingCollection<IEnumerable<QueueItem>> MessageQueue = new BlockingCollection<IEnumerable<QueueItem>>(100);

        private static readonly Task OutputTask;

        public static ILogProcessor GetDefault { get; } = new ConsoleLogProcessor();

        static ConsoleLogProcessor()
        {
            OutputTask = Task.Factory.StartNew(
                ProcessLogQueue,
                null,
                TaskCreationOptions.LongRunning);
        }

        public void EnqueueMessage(string message)
        {
            EnqueueMessage(new[] { new QueueItem { String = message } });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void EnqueueMessage(IEnumerable<QueueItem> message)
        {
            var queueItems = message as QueueItem[] ?? message.ToArray();

            if (!MessageQueue.IsAddingCompleted)
            {
                try
                {
                    MessageQueue.Add(queueItems);
                    return;
                }
                catch (InvalidOperationException)
                {

                }
            }

            WriteMessage(queueItems);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private static void WriteMessage(IEnumerable<QueueItem> message)
        {
            foreach (var queueItem in message)
            {
                if (queueItem.ForegroundColor.HasValue)
                    Console.ForegroundColor = queueItem.ForegroundColor.Value;

                if (queueItem.BackgroundColor.HasValue)
                    Console.BackgroundColor = queueItem.BackgroundColor.Value;

                Console.Write(queueItem.String);
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        private static void ProcessLogQueue(object state)
        {
            foreach (var message in MessageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }


        public void Dispose()
        {
            MessageQueue.CompleteAdding();
            try
            {
                OutputTask.Wait(1500); // with timeout in-case Console is locked by user input
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        }
    }

    public struct QueueItem
    {
        public ConsoleColor? ForegroundColor;
        public ConsoleColor? BackgroundColor;
        public string String;
    }
}
