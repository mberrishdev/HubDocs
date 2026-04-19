using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace HubDocs.UnitTests;

public class ExtensionsInternalTests
{
    [Fact]
    public async Task AddHubDocs_WhenCalled_ShouldRegisterExpectedRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSignalR();
        var app = builder.Build();

        // Act
        app.AddHubDocs();
        await app.StartAsync();

        // Assert
        var routeEndpoints = app.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => e.RoutePattern.RawText)
            .ToList();

        Assert.Contains("/hubdocs/hubdocs.json", routeEndpoints);
        Assert.Contains("/hubdocs/index.html", routeEndpoints);
        Assert.Contains("/hubdocs", routeEndpoints);

        await app.StopAsync();
    }

    [Fact]
    public async Task GetHubRoutesFromEndpoints_WhenHubMapped_ShouldReturnRoute()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSignalR();
        var app = builder.Build();
        app.MapHub<SimpleHub>("/hubs/simple");

        var method = typeof(Extensions).GetMethod(
            "GetHubRoutesFromEndpoints",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Act
        await app.StartAsync();
        var result = method!.Invoke(null, new object[] { app });
        var routes = Assert.IsAssignableFrom<Dictionary<Type, string>>(result);
        await app.StopAsync();

        // Assert
        Assert.Equal("/hubs/simple", routes[typeof(SimpleHub)]);
    }

    [Fact]
    public void DiscoverSignalRHubs_WhenStronglyTypedHubPresent_ShouldIncludeClientMethods()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "DiscoverSignalRHubs",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var routes = new Dictionary<Type, string>
        {
            { typeof(TypedHub), "/hubs/typed" },
            { typeof(UnattributedHub), "/hubs/unattributed" }
        };

        // Act
        var result = method!.Invoke(null, new object[] { routes, new[] { typeof(TypedHub).Assembly } });
        var discovered = Assert.IsAssignableFrom<IEnumerable<HubMetadata>>(result).ToList();

        // Assert
        var typed = Assert.Single(discovered, h => h.HubName == nameof(TypedHub));
        Assert.Equal("/hubs/typed", typed.Path);
        Assert.Equal(typeof(ITypedClient).FullName, typed.ClientInterfaceName);
        Assert.NotNull(typed.ClientMethods);
        Assert.Contains(typed.ClientMethods!, m => m.MethodName == nameof(ITypedClient.Notify));
        Assert.Contains(typed.Methods, m => m.MethodName == nameof(TypedHub.Send));
        Assert.DoesNotContain(discovered, h => h.HubName == nameof(UnattributedHub));
        Assert.DoesNotContain(discovered, h => h.HubName == nameof(NotRegisteredHub));
    }

    [Fact]
    public void GetMethodSignature_WhenMethodHasParameters_ShouldIncludeParameterTypeNames()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "GetMethodSignature",
            BindingFlags.NonPublic | BindingFlags.Static);
        var targetMethod = typeof(SignatureHolder).GetMethod(nameof(SignatureHolder.Work));

        Assert.NotNull(method);
        Assert.NotNull(targetMethod);

        // Act
        var signature = Assert.IsType<string>(method!.Invoke(null, new object[] { targetMethod! }));

        // Assert
        Assert.Equal("Work(System.Int32,System.String)", signature);
    }

    [Fact]
    public void FormatType_WhenTypeIsGeneric_ShouldReturnReadableGenericName()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "FormatType",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Act
        var formatted = Assert.IsType<string>(
            method!.Invoke(null, new object[] { typeof(Dictionary<string, List<int?>>) }));

        // Assert
        Assert.Equal("Dictionary<string, List<int?>>", formatted);
    }

    [Fact]
    public void FormatParameter_WhenParameterIsNullableValueType_ShouldAppendQuestionMark()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "FormatParameter",
            BindingFlags.NonPublic | BindingFlags.Static);

        var targetMethod = typeof(NullableHolder).GetMethod(nameof(NullableHolder.AcceptsNullableValue));
        var parameter = targetMethod!.GetParameters().Single();

        Assert.NotNull(method);

        // Act
        var formatted = Assert.IsType<string>(method!.Invoke(null, new object[] { parameter }));

        // Assert
        Assert.Equal("int?", formatted);
    }

    [Fact]
    public void IsNullable_WhenParameterIsNullableValueType_ShouldReturnTrue()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "IsNullable",
            BindingFlags.NonPublic | BindingFlags.Static);

        var targetMethod = typeof(NullableHolder).GetMethod(nameof(NullableHolder.AcceptsNullableValue));
        var parameter = targetMethod!.GetParameters().Single();

        Assert.NotNull(method);

        // Act
        var result = Assert.IsType<bool>(method!.Invoke(null, new object[] { parameter }));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsNullable_WhenParameterIsNonNullableReferenceType_ShouldReturnFalse()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "IsNullable",
            BindingFlags.NonPublic | BindingFlags.Static);

        var targetMethod = typeof(NullableHolder).GetMethod(nameof(NullableHolder.AcceptsNonNullableRef));
        var parameter = targetMethod!.GetParameters().Single();

        Assert.NotNull(method);

        // Act
        var result = Assert.IsType<bool>(method!.Invoke(null, new object[] { parameter }));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAllPublicHubMethods_WhenDerivedOverridesBaseByName_ShouldExcludeBaseMethodWithSameName()
    {
        // Arrange
        var method = typeof(Extensions).GetMethod(
            "GetAllPublicHubMethods",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Act
        var result = method!.Invoke(null, new object[] { typeof(DerivedHub) });
        var methods = Assert.IsAssignableFrom<IEnumerable<MethodInfo>>(result).ToList();

        // Assert
        Assert.Contains(methods, m => m.Name == nameof(DerivedHub.DerivedOnly));
        Assert.Contains(methods, m => m.Name == nameof(BaseHub.BaseOnly));
        Assert.Single(methods, m => m.Name == nameof(BaseHub.SharedName));
    }

    public class SignatureHolder
    {
        public void Work(int a, string b)
        {
        }
    }

    public class NullableHolder
    {
        public void AcceptsNullableRef(string? value)
        {
        }

        public void AcceptsNonNullableRef(string value)
        {
        }

        public void AcceptsNullableValue(int? value)
        {
        }
    }

    public class BaseHub : Hub
    {
        public virtual Task SharedName(string data) => Task.CompletedTask;

        public Task BaseOnly() => Task.CompletedTask;
    }

    public class DerivedHub : BaseHub
    {
        public override Task SharedName(string data) => Task.CompletedTask;

        public Task DerivedOnly() => Task.CompletedTask;
    }

    public interface ITypedClient
    {
        Task Notify(string message);
    }

    [HubDocs]
    public class TypedHub : Hub<ITypedClient>
    {
        public Task Send(string? message, int? code) => Task.CompletedTask;
    }

    public class UnattributedHub : Hub
    {
        public Task Ping() => Task.CompletedTask;
    }

    [HubDocs]
    public class NotRegisteredHub : Hub
    {
        public Task MissingRoute() => Task.CompletedTask;
    }

    public class SimpleHub : Hub
    {
    }
}
