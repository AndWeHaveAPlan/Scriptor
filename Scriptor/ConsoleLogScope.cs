using System;
using System.Threading;

namespace AndWeHaveAPlan.Scriptor
{
    /*
    [Obsolete]
    internal class ConsoleLogScope
    {
        private readonly string _name;
        private readonly object _state;

        internal ConsoleLogScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public ConsoleLogScope Parent { get; private set; }

        private static readonly AsyncLocal<ConsoleLogScope> Value = new AsyncLocal<ConsoleLogScope>();
        public static ConsoleLogScope Current
        {
            set => Value.Value = value;
            get => Value.Value;
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new ConsoleLogScope(name, state) { Parent = temp };

            return new DisposableScope();
        }

        public override string ToString()
        {
            return _state?.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }*/


}