﻿using System;
using System.Collections.Generic;
using System.Text;
using AndWeHaveAPlan.Scriptor.Processing;
using Microsoft.Extensions.Logging;

namespace AndWeHaveAPlan.Scriptor.Loggers
{
    /// <summary>
    /// 
    /// </summary>
    public class ColoredLogger : ScriptorLogger
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="scopeProvider"></param>
        /// <param name="logProcessor">ILogProcessorInstance</param>
        public ColoredLogger(string name, IExternalScopeProvider scopeProvider, ILogProcessor logProcessor = null) : base(name, scopeProvider, logProcessor ?? new ConsoleLogProcessor())
        { }

        protected override List<QueueItem> ComposeInternal(LogMessage message)
        {
            var result = new List<QueueItem>
            {
                new QueueItem
                {
                    ForegroundColor = ConsoleColor.Green,
                    String = $"[ {message.LevelString} | {message.Level} | {message.Timestamp} ] {Name}:\n"
                }
            };


            if (IncludeScopes && !string.IsNullOrEmpty(message.Scope))
            {
                result.Add(new QueueItem
                {
                    ForegroundColor = ConsoleColor.Cyan,
                    String = message.Scope
                });

                result.Add(new QueueItem
                {
                    String = "\n"
                });
            }

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


            if (message.Exception != null)
            {
                result.Add(new QueueItem
                {
                    String = "\n"
                });

                result.Add(new QueueItem
                {
                    ForegroundColor = ConsoleColor.Red,
                    String = message.Exception
                });
            }

            result.Add(new QueueItem
            {
                String = "\n"
            });

            return result;
        }
    }
}