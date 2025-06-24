namespace HubDocs.UnitTests;

public class HubMethodMetadataTests
{
    [Fact]
    public void HubMethodMetadata_WhenCreated_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var metadata = new HubMethodMetadata();
        
        // Assert
        Assert.NotNull(metadata.ParameterTypes);
        Assert.Empty(metadata.ParameterTypes);
        Assert.Null(metadata.MethodName);
        Assert.Null(metadata.ReturnType);
    }

    [Fact]
    public void HubMethodMetadata_WhenPropertiesSet_ShouldReturnValues()
    {
        // Arrange
        var paramTypes = new List<string> { "int", "string" };
        
        // Act
        var metadata = new HubMethodMetadata
        {
            MethodName = "Method",
            ParameterTypes = paramTypes,
            ReturnType = "void"
        };
        
        // Assert
        Assert.Equal("Method", metadata.MethodName);
        Assert.Equal(paramTypes, metadata.ParameterTypes);
        Assert.Equal("void", metadata.ReturnType);
    }
}