using System.Collections.Generic;

namespace AndWeHaveAPlan.Scriptor.Processing
{
    public interface ILogProcessor
    {
        void EnqueueMessage(IEnumerable<QueueItem> message);
        void EnqueueMessage(string message);
    }
}