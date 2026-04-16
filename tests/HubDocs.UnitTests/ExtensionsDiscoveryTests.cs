using System.Reflection;
using Microsoft.AspNetCore.SignalR;

namespace HubDocs.UnitTests;

public class ExtensionsDiscoveryTests
{
    [Fact]
    public void DiscoverSignalRHubs_WhenAssemblyContainsNonAttributedHub_ShouldNotThrowAndReturnAttributedHub()
    {
        // Arrange
        var hubRoutes = new Dictionary<Type, string>
        {
            { typeof(AttributedHub), "/hubs/attributed" },
            { typeof(NonAttributedHub), "/hubs/non-attributed" }
        };

        var method = typeof(Extensions).GetMethod(
            "DiscoverSignalRHubs",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Act
        var result = method!.Invoke(null, new object[] { hubRoutes, new[] { typeof(AttributedHub).Assembly } });
        var discovered = Assert.IsAssignableFrom<IEnumerable<HubMetadata>>(result).ToList();

        // Assert
        var single = Assert.Single(discovered);
        Assert.Equal(nameof(AttributedHub), single.HubName);
        Assert.Equal("/hubs/attributed", single.Path);
    }

    [HubDocs]
    private class AttributedHub : Hub
    {
    }

    private class NonAttributedHub : Hub
    {
    }
}
