using Microsoft.AspNetCore.SignalR;

namespace HubDocs;

public static class HubRouteRegistry 
{
    private static readonly List<HubMapping> Mappings = [];

    public static void AddMapping<T>(string path) where T : Hub
    {
        Mappings.Add(new HubMapping
        {
            HubType = typeof(T),
            Path = path
        });
    }

    public static IReadOnlyList<HubMapping> GetMappings() => Mappings.AsReadOnly();
}

public class HubMapping
{
    public Type HubType { get; init; } = null!;
    public string Path { get; init; } = null!;
}