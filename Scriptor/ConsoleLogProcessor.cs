using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AndWeHaveAPlan.Scriptor
{
    internal class ConsoleLogProcessor : IDisposable
    {
        private static readonly BlockingCollection<QueueItem[]> _messageQueue = new BlockingCollection<QueueItem[]>();

        private static readonly Task _outputTask;

        /// <summary>
        /// 
        /// </summary>
        static ConsoleLogProcessor()
        {
            _outputTask = Task.Factory.StartNew(
                ProcessLogQueue,
                null,
                TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public virtual void EnqueueMessage(QueueItem[] message)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException)
                {

                }
            }

            // Adding is completed so just log the message
            WriteMessage(message);
        }

        private static void WriteMessage(QueueItem[] message)
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

        private void ProcessLogQueue()
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }

        private static void ProcessLogQueue(object state)
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
