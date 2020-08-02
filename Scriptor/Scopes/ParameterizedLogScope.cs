using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndWeHaveAPlan.Scriptor.Scopes
{
    /// <summary>
    /// Wrapper for (string, object) ( ValueTuple&lt;string, object&gt; ) for type checking
    /// </summary>
    internal class ParameterizedLogScopeItem
    {
        public ParameterizedLogScopeItem(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public ParameterizedLogScopeItem(ValueTuple<string, object> tuple)
        {
            Key = tuple.Item1;
            Value = tuple.Item2;
        }

        public string Key;
        public object Value;

        public override string ToString()
        {
            return $"{Key}:{Value}";
        }

        public static implicit operator ParameterizedLogScopeItem(ValueTuple<string, object> tuple)
        {
            return new ParameterizedLogScopeItem(tuple.Item1, tuple.Item2);
        }

        public static implicit operator ValueTuple<string, object>(ParameterizedLogScopeItem scopeLogItem)
        {
            return (scopeLogItem.Key, scopeLogItem.Value);
        }
    }

    /// <summary>
    /// IEnumerable Wrapper for (string, object) ( ValueTuple&lt;string, object&gt; ) for type checking
    /// </summary>
    internal class ParameterizedLogScope : IEnumerable<(string, object)>
    {
        private readonly List<(string, object)> _items;

        public ParameterizedLogScope((string, object)[] items)
        {
            _items = new List<(string, object)>(items);
        }

        public IEnumerator<(string, object)> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var firstItem = _items.First();
            var builder = new StringBuilder($"{firstItem.Item1}: {firstItem.Item2}");

            foreach (var (key, value) in _items.Skip(1))
            {
                builder.Append($", {key}:{value}");
            }

            return builder.ToString();
        }
    }
}
