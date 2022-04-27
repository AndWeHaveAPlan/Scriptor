using System;

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
            return $"{Key}: {Value}";
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
}