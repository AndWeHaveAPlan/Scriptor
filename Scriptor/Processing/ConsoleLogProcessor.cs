using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Scriptor.Processing
{
    public sealed class ConsoleLogProcessor : IDisposable, ILogProcessor
    {
        private readonly BlockingCollection<IEnumerable<QueueItem>> _messageQueue = new BlockingCollection<IEnumerable<QueueItem>>(100);

        private readonly Task _outputTask;

        /// <summary>
        /// 
        /// </summary>
        public ConsoleLogProcessor()
        {
            _outputTask = Task.Factory.StartNew(
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

            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(queueItems);
                    return;
                }
                catch (InvalidOperationException)
                {

                }
            }

            WriteMessage(queueItems);
        }

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

        private void ProcessLogQueue(object state)
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }


        public void Dispose()
        {
            _messageQueue.CompleteAdding();
            try
            {
                _outputTask.Wait(1500); // with timeout in-case Console is locked by user input
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
