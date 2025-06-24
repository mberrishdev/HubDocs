using Microsoft.AspNetCore.SignalR;

namespace HubDocs.UnitTests;

public class HubRouteRegistryTests
{
    [Fact]
    public void AddMapping_WhenCalled_ShouldAddMapping()
    {
        // Arrange
        ClearMappings();
        
        // Act
        HubRouteRegistry.AddMapping<TestHub>("/test");
        
        // Assert
        var mapping = HubRouteRegistry.GetMappings().FirstOrDefault(m => m.HubType == typeof(TestHub));
        Assert.NotNull(mapping);
        Assert.Equal("/test", mapping.Path);
        
        // Cleanup
        ClearMappings();
    }

    [Fact]
    public void GetMappings_WhenCalled_ShouldReturnAllMappings()
    {
        // Arrange
        ClearMappings();
        HubRouteRegistry.AddMapping<TestHub>("/test");
        
        // Act
        var mappings = HubRouteRegistry.GetMappings();
        
        // Assert
        Assert.Single(mappings);
        Assert.Equal(typeof(TestHub), mappings[0].HubType);
        
        // Cleanup
        ClearMappings();
    }

    [Fact]
    public void HubMapping_WhenPropertiesSet_ShouldReturnValues()
    {
        // Arrange
        var mapping = new HubMapping { HubType = typeof(TestHub), Path = "/abc" };
        
        // Act & Assert
        Assert.Equal(typeof(TestHub), mapping.HubType);
        Assert.Equal("/abc", mapping.Path);
    }

    private static void ClearMappings()
    {
        var field = typeof(HubRouteRegistry).GetField("_mappings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var list = (System.Collections.IList)field!.GetValue(null)!;
        list!.Clear();
    }

    private class TestHub : Hub { }
}