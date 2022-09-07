using System;
using System.Collections.Generic;
using System.Linq;

namespace lib;

public static class DictionaryExtensions
{
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
        else
            dictionary[key] = value;
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, Func<TValue, TValue> update)
    {
        if (dictionary.TryGetValue(key, out var currentValue))
            dictionary[key] = update(currentValue);
        else
            dictionary[key] = value;
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, Action<TValue> update)
    {
        if (dictionary.TryGetValue(key, out var currentValue))
            update(currentValue);
        else
            dictionary[key] = value;
    }

    public static void Increase<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int delta = 1)
    {
        if (!dictionary.TryGetValue(key, out var v))
            v = 0;
        dictionary[key] = v+delta;
    }

    public static string ToRatingString<TKey>(this IDictionary<TKey, int> dictionary)
    {
        return dictionary.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Value,6}: {kv.Key}").StrJoin("\n");
    }

    public static string ToRatingString<TKey>(this IDictionary<TKey, StatValue> dictionary)
    {
        return dictionary.OrderByDescending(kv => kv.Value.Sum).Select(kv => $"{kv.Key}: {kv.Value.Sum}  {kv.Value.ToDetailedString()}").StrJoin("\n");
    }
}
