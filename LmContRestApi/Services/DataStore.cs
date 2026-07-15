using System.Collections.Concurrent;

namespace LmContRestApi.Services;

public static class DataStore
{
    private static readonly ConcurrentDictionary<string, object> _store = new();

    public static void Set(string typeName, object instance) => _store[typeName] = instance;

    public static object? Get(string typeName) => _store.TryGetValue(typeName, out var val) ? val : null;

    public static void Clear() => _store.Clear();


}