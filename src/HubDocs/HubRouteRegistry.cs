using Microsoft.AspNetCore.SignalR;

namespace HubDocs;

public static class HubRouteRegistry 
{
    private static readonly List<HubMapping> _mappings = [];

    public static void AddMapping<T>(string path) where T : Hub
    {
        _mappings.Add(new HubMapping
        {
            HubType = typeof(T),
            Path = path
        });
    }

    public static IReadOnlyList<HubMapping> GetMappings() => _mappings.AsReadOnly();
}

public class HubMapping
{
    public Type HubType { get; set; } = default!;
    public string Path { get; set; } = default!;
}