using Microsoft.AspNetCore.SignalR;

namespace HubDocs.UnitTests;

public class HubRouteRegistryTests
{
    [Fact]
    public void HubMapping_WhenPropertiesSet_ShouldReturnValues()
    {
        // Arrange
        var mapping = new HubMapping { HubType = typeof(TestHub), Path = "/abc" };

        // Act & Assert
        Assert.Equal(typeof(TestHub), mapping.HubType);
        Assert.Equal("/abc", mapping.Path);
    }

    private class TestHub : Hub { }
}