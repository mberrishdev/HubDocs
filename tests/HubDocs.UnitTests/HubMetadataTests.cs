namespace HubDocs.UnitTests;

public class HubMetadataTests
{
    [Fact]
    public void HubMetadata_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var metadata = new HubMetadata();
        
        // Assert
        Assert.NotNull(metadata.Methods);
        Assert.Empty(metadata.Methods);
        Assert.Null(metadata.ClientInterfaceName);
        Assert.Null(metadata.ClientMethods);
    }

    [Fact]
    public void HubMetadata_WhenPropertiesSet_ShouldReturnValues()
    {
        // Arrange
        var methods = new List<HubMethodMetadata> { new() { MethodName = "A" } };
        var clientMethods = new List<HubMethodMetadata> { new() { MethodName = "B" } };
        
        // Act
        var metadata = new HubMetadata
        {
            HubName = "Hub",
            HubFullName = "Namespace.Hub",
            Path = "/hub",
            Methods = methods,
            ClientInterfaceName = "IClient",
            ClientMethods = clientMethods
        };
        
        // Assert
        Assert.Equal("Hub", metadata.HubName);
        Assert.Equal("Namespace.Hub", metadata.HubFullName);
        Assert.Equal("/hub", metadata.Path);
        Assert.Equal(methods, metadata.Methods);
        Assert.Equal("IClient", metadata.ClientInterfaceName);
        Assert.Equal(clientMethods, metadata.ClientMethods);
    }
}