using System;
using System.Collections.Concurrent;

public class InMemoryDatabase
{
    private readonly ConcurrentDictionary<string, object> _store = new ConcurrentDictionary<string, object>();

    public bool Set(string key, object value)
    {
        return _store.TryAdd(key, value);
    }

    public bool Get(string key, out object value)
    {
        return _store.TryGetValue(key, out value);
    }

    public bool Remove(string key)
    {
        return _store.TryRemove(key, out _);
    }
}
