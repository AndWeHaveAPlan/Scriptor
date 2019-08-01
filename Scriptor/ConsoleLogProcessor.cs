using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Scriptor
{
    internal class ConsoleLogProcessor : IDisposable
    {
        private static readonly BlockingCollection<LogMessage> _messageQueue = new BlockingCollection<LogMessage>();

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
        public virtual void EnqueueMessage(LogMessage message)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(message);
                    return;
                }
                catch (InvalidOperationException) { }
            }

            // Adding is completed so just log the message
            WriteMessage(message);
        }

        private static void WriteMessage(LogMessage message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message.Header);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message.Scope);

            Console.ResetColor();
            Console.WriteLine(message.Message);

            if (message.Exception != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message.Exception);
            }
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
            //var consoleLogger = (ConsoleLogProcessor)state;
            //consoleLogger.ProcessLogQueue();

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
}
