using Microsoft.AspNetCore.SignalR;

namespace HubDocs.UnitTests;

public class HubRouteRegistryStateTests
{
    [Fact]
    public void AddMapping_WhenCalled_ShouldAppendMapping()
    {
        // Arrange
        var before = HubRouteRegistry.GetMappings().Count;
        var path = $"/hubs/state-{Guid.NewGuid():N}";

        // Act
        HubRouteRegistry.AddMapping<TestHub>(path);
        var after = HubRouteRegistry.GetMappings();

        // Assert
        Assert.Equal(before + 1, after.Count);
        var added = after.Last();
        Assert.Equal(typeof(TestHub), added.HubType);
        Assert.Equal(path, added.Path);
    }

    [Fact]
    public void GetMappings_WhenCalled_ShouldReturnReadOnlyCollection()
    {
        // Act
        var mappings = HubRouteRegistry.GetMappings();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<HubMapping>>(mappings);
    }

    private class TestHub : Hub
    {
    }
}
