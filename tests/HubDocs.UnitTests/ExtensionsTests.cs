using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace HubDocs.UnitTests;

public class ExtensionsTests
{
    [Fact]
    public void DiscoverSignalRHubs_WhenAttributedHubIsNotRegistered_ShouldSkipWithoutThrowing()
    {
        var method = typeof(Extensions).GetMethod("DiscoverSignalRHubs", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var hubRoutes = new Dictionary<Type, string>();
        var result = (IEnumerable<HubMetadata>)method!.Invoke(null, [hubRoutes, new[] { typeof(DocumentedHub).Assembly }])!;

        Assert.Empty(result.ToList());
    }

    [Fact]
    public void DiscoverSignalRHubs_WhenAttributedHubIsRegistered_ShouldReturnHubMetadata()
    {
        var method = typeof(Extensions).GetMethod("DiscoverSignalRHubs", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var hubRoutes = new Dictionary<Type, string>
        {
            [typeof(DocumentedHub)] = "/hubs/documented"
        };

        var result = (IEnumerable<HubMetadata>)method!.Invoke(null, [hubRoutes, new[] { typeof(DocumentedHub).Assembly }])!;
        var hubs = result.ToList();

        Assert.Single(hubs);
        Assert.Equal(nameof(DocumentedHub), hubs[0].HubName);
        Assert.Equal("/hubs/documented", hubs[0].Path);
    }

    [HubDocs]
    private class DocumentedHub : Hub
    {
    }
}
