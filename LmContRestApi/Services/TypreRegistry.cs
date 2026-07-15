using SharedModels;

namespace LmContRestApi.Services;

public static class TypeRegistry
{
    private static readonly Dictionary<string, Type> _map = new()
    {
        { nameof(CityData), typeof(CityData) },
    };

    public static Type? Resolve(string typeName)
        => _map.TryGetValue(typeName, out var t) ? t : null;
}