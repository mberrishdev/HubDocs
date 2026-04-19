namespace HubDocs.UnitTests;

public class HubDocsAttributeTests
{
    [Fact]
    public void HubDocsAttribute_WhenQueried_ShouldHaveExpectedUsageMetadata()
    {
        // Arrange
        var usage = (AttributeUsageAttribute?)Attribute.GetCustomAttribute(
            typeof(HubDocsAttribute),
            typeof(AttributeUsageAttribute));

        // Assert
        Assert.NotNull(usage);
        Assert.Equal(AttributeTargets.Class, usage!.ValidOn);
        Assert.False(usage.AllowMultiple);
        Assert.False(usage.Inherited);
    }

    [Fact]
    public void HubDocsAttribute_WhenCreated_ShouldInstantiateSuccessfully()
    {
        // Act
        var attribute = new HubDocsAttribute();

        // Assert
        Assert.NotNull(attribute);
    }
}
