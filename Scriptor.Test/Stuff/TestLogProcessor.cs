using System.Collections.Generic;
using AndWeHaveAPlan.Scriptor.Processing;

namespace AndWeHaveAPlan.Scriptor.Test.Stuff
{
    public class TestLogProcessor : ILogProcessor
    {
        public List<IEnumerable<QueueItem>> Log = new List<IEnumerable<QueueItem>>();

        public void EnqueueMessage(IEnumerable<QueueItem> message)
        {
            Log.Add(message);
        }

        public void EnqueueMessage(string message)
        {
            Log.Add(new[] { new QueueItem { String = message } });
        }
    }
}
