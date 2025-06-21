using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;


namespace HubDocs;

public static class SignalRDiscoveryExtensions
{
    public static void MapHubAndRegister<T>(this IEndpointRouteBuilder endpoints, string pattern)
        where T : Hub
    {
        endpoints.MapHub<T>(pattern);
        HubRouteRegistry.AddMapping<T>(pattern);
    }

    public static WebApplication AddHubDocs(this WebApplication app)
    {
        var assemblies = HubRouteRegistry.GetMappings()
            .Select(m => m.HubType.Assembly)
            .Distinct()
            .ToArray();

        app.MapGet("/hubdocs/hubdocs.json", () =>
            {
                var metadata = assemblies.DiscoverSignalRHubs();
                return Results.Ok(metadata);
            })
            .ExcludeFromDescription();

        app.MapGet("/hubdocs/index.html", async context =>
        {
            var assembly = typeof(SignalRDiscoveryExtensions).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("hubdocs.html"));

            if (resourceName == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("hubdocs.html not found in resources.");
                return;
            }

            context.Response.ContentType = "text/html";
            await using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            await context.Response.WriteAsync(html);
        });

        app.MapGet("/hubdocs", context =>
        {
            context.Response.Redirect("/hubdocs/index.html", permanent: false);
            return Task.CompletedTask;
        }).ExcludeFromDescription();

        return app;
    }

    private static IEnumerable<HubMetadata> DiscoverSignalRHubs(this IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(Hub).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(hubType =>
            {
                var mapping = HubRouteRegistry.GetMappings()
                    .FirstOrDefault(m => m.HubType == hubType);

                var hubMetadata = new HubMetadata
                {
                    HubName = hubType.Name,
                    HubFullName = hubType.FullName!,
                    Path = mapping?.Path,
                    Methods = GetAllPublicHubMethods(hubType)
                        .GroupBy(GetMethodSignature)
                        .Select(g => g.First())
                        .Select(m => new HubMethodMetadata
                        {
                            MethodName = m.Name,
                            ParameterTypes = m.GetParameters()
                                .Select(FormatParameter)
                                .ToList(),
                            ReturnType = FormatType(m.ReturnType)
                        })
                        .ToList()
                };

                if (hubType.BaseType?.IsGenericType != true ||
                    hubType.BaseType.GetGenericTypeDefinition() != typeof(Hub<>)) return hubMetadata;

                var clientInterface = hubType.BaseType.GetGenericArguments()[0];

                hubMetadata.ClientInterfaceName = clientInterface.FullName!;
                hubMetadata.ClientMethods = clientInterface.GetMethods()
                    .Select(m => new HubMethodMetadata
                    {
                        MethodName = m.Name,
                        ParameterTypes = m.GetParameters()
                            .Select(FormatParameter)
                            .ToList(),
                        ReturnType = FormatType(m.ReturnType)
                    })
                    .ToList();

                return hubMetadata;
            })
            .DistinctBy(x => x.HubName);
    }

    private static IEnumerable<MethodInfo> GetAllPublicHubMethods(Type type)
    {
        var allMethods = new List<MethodInfo>();
        var methodNamesFromDerived = new HashSet<string>();

        var declaredMethods = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && m.Name != nameof(IDisposable.Dispose))
            .ToList();

        foreach (var m in declaredMethods)
            methodNamesFromDerived.Add(m.Name);

        allMethods.AddRange(declaredMethods);

        var current = type.BaseType;
        while (current != null && typeof(Hub).IsAssignableFrom(current))
        {
            var baseMethods = current
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m =>
                    !m.IsSpecialName &&
                    m.Name != nameof(IDisposable.Dispose) &&
                    !methodNamesFromDerived.Contains(m.Name)
                );

            allMethods.AddRange(baseMethods);
            current = current.BaseType;
        }

        return allMethods;
    }

    private static string GetMethodSignature(MethodInfo method)
    {
        var paramTypes = method.GetParameters()
            .Select(p => p.ParameterType.FullName)
            .ToArray();

        return $"{method.Name}({string.Join(",", paramTypes)})";
    }

    private static string FormatType(Type type)
    {
        if (!type.IsGenericType)
            return type.Name;

        var typeName = type.Name[..type.Name.IndexOf('`')];
        var genericArgs = type.GetGenericArguments()
            .Select(FormatType);
        return $"{typeName}<{string.Join(", ", genericArgs)}>";
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        var typeName = FormatType(type);

        bool isNullable = IsNullable(parameter);

        return isNullable ? $"{typeName}?" : typeName;
    }

    private static bool IsNullable(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;

        if (Nullable.GetUnderlyingType(type) != null)
            return true;

        var nullableAttr = parameter
            .CustomAttributes
            .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

        if (nullableAttr is { ConstructorArguments.Count: 1 })
        {
            var arg = nullableAttr.ConstructorArguments[0];

            if (arg.ArgumentType == typeof(byte) && (byte)arg.Value! == 2)
                return true;

            if (arg.ArgumentType == typeof(CustomAttributeTypedArgument[]) &&
                ((ReadOnlyCollection<CustomAttributeTypedArgument>)arg.Value).FirstOrDefault().Value is byte and 2)
                return true;
        }

        return false;
    }
}