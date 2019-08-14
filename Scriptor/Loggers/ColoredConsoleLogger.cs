using System;
using System.Collections.Generic;
using System.Text;

namespace Scriptor.Loggers
{
    public class ColoredConsoleLogger : ScriptorLogger
    {
        /// <summary>
        /// 
        /// </summary>
        static ColoredConsoleLogger()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="includeScopes"></param>
        public ColoredConsoleLogger(string name, bool includeScopes = false) : base(name, includeScopes)
        { }

        protected override List<QueueItem> ComposeInternal(LogMessage message)
        {
            var result = new List<QueueItem>();

            result.Add(new QueueItem
            {
                ForegroundColor = ConsoleColor.Green,
                String = $"[ {message.LevelString} | {message.Level} | {message.Timestamp:O} ] {Name}:\n"
            });

            if (IncludeScopes)
                result.Add(new QueueItem
                {
                    ForegroundColor = ConsoleColor.Yellow,
                    String = message.Scope
                });

            result.Add(new QueueItem
            {
                String = "\n"
            });

            if (message.AuxData != null)
            {
                var stringBuilder = new StringBuilder();
                foreach (var (key, value) in message.AuxData)
                {
                    stringBuilder.Append(key);
                    stringBuilder.Append(": ");
                    stringBuilder.Append(value);
                    stringBuilder.Append("\n");
                }

                result.Add(new QueueItem
                {
                    ForegroundColor = ConsoleColor.Yellow,
                    String = stringBuilder.ToString()
                });
            }

            result.Add(new QueueItem
            {
                String = message.Message
            });

            result.Add(new QueueItem
            {
                String = "\n"
            });

            if (message.Exception != null)
                result.Add(new QueueItem
                {
                    ForegroundColor = ConsoleColor.Red,
                    String = message.Exception
                });

            return result;
        }
    }
}