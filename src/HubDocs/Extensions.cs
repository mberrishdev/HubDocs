using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace HubDocs;

public static class Extensions
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public static WebApplication AddHubDocs(this WebApplication app, params Assembly[] additionalAssemblies)
    {
        app.MapGet("/hubdocs/hubdocs.json", () =>
            {
                var hubRoutes = GetHubRoutesFromEndpoints(app);
                var metadata = DiscoverSignalRHubs(hubRoutes, additionalAssemblies);
                return Results.Ok(metadata);
            })
            .ExcludeFromDescription();

        app.MapGet("/hubdocs/index.html", async context =>
        {
            var assembly = typeof(Extensions).Assembly;
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

    private static Dictionary<Type, string> GetHubRoutesFromEndpoints(WebApplication app)
    {
        var hubRoutes = new Dictionary<Type, string>();
        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();

        foreach (var endpoint in dataSource.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint) continue;
            foreach (var metadata in routeEndpoint.Metadata)
            {
                var metadataType = metadata.GetType();
                if (metadataType.FullName == "Microsoft.AspNetCore.SignalR.HubMetadata")
                {
                    var hubTypeProperty = metadataType.GetProperty("HubType");
                    if (hubTypeProperty?.GetValue(metadata) is Type hubType)
                    {
                        var pattern = routeEndpoint.RoutePattern.RawText;
                        if (!string.IsNullOrEmpty(pattern))
                        {
                            hubRoutes[hubType] = pattern;
                        }
                    }
                }
            }
        }

        return hubRoutes;
    }

    private static IEnumerable<HubMetadata> DiscoverSignalRHubs(Dictionary<Type, string> hubRoutes,
        params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Length > 0
            ? assemblies
            : AppDomain.CurrentDomain.GetAssemblies();

        return assembliesToScan
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    return [];
                }
            })
            .Where(t => typeof(Hub).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(hubType =>
            {
                var attribute = hubType.GetCustomAttribute<HubDocsAttribute>();
                if (attribute == null || !hubRoutes.ContainsKey(hubType))
                    return null;

                var schemaRegistry = new Dictionary<string, HubTypeSchemaMetadata>();

                var hubMetadata = new HubMetadata
                {
                    HubName = hubType.Name,
                    HubFullName = hubType.FullName!,
                    Path = hubRoutes[hubType],
                    Methods = [.. GetAllPublicHubMethods(hubType)
                        .GroupBy(GetMethodSignature)
                        .Select(g => g.First())
                        .Select(m => BuildHubMethodMetadata(m, schemaRegistry))],
                    Schemas = []
                };

                if (hubType.BaseType?.IsGenericType == true &&
                    hubType.BaseType.GetGenericTypeDefinition() == typeof(Hub<>))
                {
                    var clientInterface = hubType.BaseType.GetGenericArguments()[0];

                    hubMetadata.ClientInterfaceName = clientInterface.FullName!;
                    hubMetadata.ClientMethods = [.. clientInterface.GetMethods()
                        .Select(m => BuildHubMethodMetadata(m, schemaRegistry))];
                }

                hubMetadata.Schemas = [.. schemaRegistry.Values.OrderBy(s => s.Name)];

                return hubMetadata;
            })
            .OfType<HubMetadata>()
            .Where(h => h.Path != null)
            .DistinctBy(x => x.HubName);
    }

    private static HubMethodMetadata BuildHubMethodMetadata(MethodInfo method, Dictionary<string, HubTypeSchemaMetadata> schemaRegistry)
    {
        var parameters = method.GetParameters()
            .Select(p => BuildParameterMetadata(p, schemaRegistry))
            .ToList();

        RegisterSchemaFromType(method.ReturnType, schemaRegistry);

        var returnType = FormatMethodReturnType(method);
        var signatureParams = string.Join(", ", parameters.Select(p => $"{p.Type} {p.Name}"));

        return new HubMethodMetadata
        {
            MethodName = method.Name,
            Signature = $"{returnType} {method.Name}({signatureParams})",
            ParameterTypes = [.. parameters.Select(p => p.Type)],
            Parameters = parameters,
            ReturnType = returnType,
            ExampleInvocation = $"{method.Name}({string.Join(", ", parameters.Select(p => p.Example))})",
            ReturnExample = BuildReturnExample(method)
        };
    }

    private static HubParameterMetadata BuildParameterMetadata(ParameterInfo parameter, Dictionary<string, HubTypeSchemaMetadata> schemaRegistry)
    {
        var isNullable = IsNullable(parameter);
        RegisterSchemaFromType(parameter.ParameterType, schemaRegistry);

        return new HubParameterMetadata
        {
            Name = parameter.Name ?? "value",
            Type = FormatTypeWithNullability(parameter.ParameterType, isNullable),
            IsNullable = isNullable,
            Example = CreateExampleLiteral(parameter.ParameterType, isNullable)
        };
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

    private static string FormatMethodReturnType(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(void))
            return "void";

        if (returnType == typeof(Task))
            return "Task";

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = returnType.GetGenericArguments()[0];
            return $"Task<{FormatType(inner)}>";
        }

        return FormatType(returnType);
    }

    private static string FormatType(Type type)
    {
        return FormatTypeWithNullability(type, false);
    }

    private static string FormatTypeWithNullability(Type type, bool nullableReference)
    {
        var nullableUnderlying = Nullable.GetUnderlyingType(type);
        if (nullableUnderlying != null)
            return $"{FormatType(nullableUnderlying)}?";

        if (type.IsArray)
            return $"{FormatType(type.GetElementType()!)}[]";

        if (TryGetKeywordAlias(type, out var alias))
            return alias;

        if (!type.IsGenericType)
        {
            if (!type.IsValueType && nullableReference)
                return $"{type.Name}?";

            return type.Name;
        }

        var typeName = type.Name[..type.Name.IndexOf('`')];
        var genericArgs = type.GetGenericArguments()
            .Select(arg => FormatType(arg));
        var formatted = $"{typeName}<{string.Join(", ", genericArgs)}>";

        if (!type.IsValueType && nullableReference)
            return $"{formatted}?";

        return formatted;
    }

    private static bool TryGetKeywordAlias(Type type, out string alias)
    {
        alias = type.Name;

        if (type == typeof(bool)) alias = "bool";
        else if (type == typeof(byte)) alias = "byte";
        else if (type == typeof(sbyte)) alias = "sbyte";
        else if (type == typeof(short)) alias = "short";
        else if (type == typeof(ushort)) alias = "ushort";
        else if (type == typeof(int)) alias = "int";
        else if (type == typeof(uint)) alias = "uint";
        else if (type == typeof(long)) alias = "long";
        else if (type == typeof(ulong)) alias = "ulong";
        else if (type == typeof(float)) alias = "float";
        else if (type == typeof(double)) alias = "double";
        else if (type == typeof(decimal)) alias = "decimal";
        else if (type == typeof(char)) alias = "char";
        else if (type == typeof(string)) alias = "string";
        else if (type == typeof(object)) alias = "object";
        else return false;

        return true;
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var isNullable = IsNullable(parameter);
        return FormatTypeWithNullability(parameter.ParameterType, isNullable);
    }

    private static bool IsSimpleType(Type type)
    {
        var normalized = NormalizeType(type);

        if (normalized.IsEnum)
            return false;

        if (TryGetKeywordAlias(normalized, out _))
            return true;

        return normalized == typeof(DateTime) ||
               normalized == typeof(DateTimeOffset) ||
               normalized == typeof(Guid) ||
               normalized == typeof(TimeSpan);
    }

    private static Type NormalizeType(Type type)
    {
        if (type == typeof(Task))
            return typeof(void);

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            return type.GetGenericArguments()[0];

        if (Nullable.GetUnderlyingType(type) is Type nullableUnderlying)
            return nullableUnderlying;

        if (type.IsArray)
            return type.GetElementType()!;

        if (type.IsGenericType && type != typeof(string))
        {
            var genericDefinition = type.GetGenericTypeDefinition();
            if (genericDefinition == typeof(IEnumerable<>) ||
                genericDefinition == typeof(ICollection<>) ||
                genericDefinition == typeof(IList<>) ||
                genericDefinition == typeof(List<>) ||
                genericDefinition == typeof(IReadOnlyList<>))
            {
                return type.GetGenericArguments()[0];
            }
        }

        return type;
    }

    private static void RegisterSchemaFromType(Type type, Dictionary<string, HubTypeSchemaMetadata> schemaRegistry)
    {
        RegisterSchemaFromType(type, schemaRegistry, new HashSet<string>());
    }

    private static void RegisterSchemaFromType(Type type, Dictionary<string, HubTypeSchemaMetadata> schemaRegistry, HashSet<string> visiting)
    {
        var normalized = NormalizeType(type);
        if (normalized == typeof(void) || IsSimpleType(normalized))
            return;

        // Skip framework/system types (for example Exception) to keep schema output focused on app DTOs.
        if (normalized.Namespace?.StartsWith("System", StringComparison.Ordinal) == true)
            return;

        var key = normalized.FullName ?? normalized.Name;
        if (visiting.Contains(key) || schemaRegistry.ContainsKey(key))
            return;

        visiting.Add(key);

        if (normalized.IsEnum)
        {
            var names = Enum.GetNames(normalized)
                .Select(n => $"{n} = {Convert.ToInt64(Enum.Parse(normalized, n))}")
                .ToList();

            schemaRegistry[key] = new HubTypeSchemaMetadata
            {
                Name = normalized.Name,
                FullName = key,
                Kind = "enum",
                EnumValues = names,
                Example = names.FirstOrDefault() ?? string.Empty
            };

            return;
        }

        if (normalized.IsClass || (normalized.IsValueType && !normalized.IsPrimitive))
        {
            var properties = normalized
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .Select(p =>
                {
                    var nullability = NullabilityContext.Create(p);
                    var isNullable = nullability.ReadState == NullabilityState.Nullable ||
                                     Nullable.GetUnderlyingType(p.PropertyType) != null;

                    RegisterSchemaFromType(p.PropertyType, schemaRegistry, visiting);

                    return new HubSchemaPropertyMetadata
                    {
                        Name = p.Name,
                        Type = FormatTypeWithNullability(p.PropertyType, isNullable),
                        IsNullable = isNullable,
                        Example = CreateExampleLiteral(p.PropertyType, isNullable)
                    };
                })
                .ToList();

            schemaRegistry[key] = new HubTypeSchemaMetadata
            {
                Name = normalized.Name,
                FullName = key,
                Kind = "object",
                Properties = properties,
                Example = CreateObjectExample(properties)
            };
        }
    }

    private static string CreateObjectExample(IReadOnlyList<HubSchemaPropertyMetadata> properties)
    {
        if (properties.Count == 0)
            return "{}";

        var lines = properties.Select(p => $"  \"{p.Name}\": {p.Example}");
        return "{\n" + string.Join(",\n", lines) + "\n}";
    }

    private static string BuildReturnExample(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(void) || returnType == typeof(Task))
            return "void";

        var unwrapped = NormalizeType(returnType);
        return CreateExampleLiteral(unwrapped, false);
    }

    private static string CreateExampleLiteral(Type type, bool nullable)
    {
        if (nullable)
            return "null";

        var normalized = NormalizeType(type);

        if (normalized == typeof(string)) return "\"example\"";
        if (normalized == typeof(bool)) return "true";
        if (normalized == typeof(byte) || normalized == typeof(sbyte) || normalized == typeof(short) ||
            normalized == typeof(ushort) || normalized == typeof(int) || normalized == typeof(uint) ||
            normalized == typeof(long) || normalized == typeof(ulong)) return "123";
        if (normalized == typeof(float) || normalized == typeof(double) || normalized == typeof(decimal)) return "12.34";
        if (normalized == typeof(char)) return "\"A\"";
        if (normalized == typeof(Guid)) return "\"3fa85f64-5717-4562-b3fc-2c963f66afa6\"";
        if (normalized == typeof(DateTime) || normalized == typeof(DateTimeOffset)) return "\"2026-01-01T00:00:00Z\"";
        if (normalized == typeof(TimeSpan)) return "\"00:30:00\"";

        if (normalized.IsEnum)
        {
            var first = Enum.GetNames(normalized).FirstOrDefault();
            return first is null ? "0" : $"\"{first}\"";
        }

        if (type.IsArray ||
            (type.IsGenericType &&
             (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
              type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
              type.GetGenericTypeDefinition() == typeof(IList<>) ||
              type.GetGenericTypeDefinition() == typeof(List<>) ||
              type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))))
        {
            return "[]";
        }

        return "{}";
    }

    private static bool IsNullable(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;

        if (Nullable.GetUnderlyingType(type) != null)
            return true;

        var nullability = NullabilityContext.Create(parameter);
        if (nullability.ReadState == NullabilityState.Nullable)
            return true;

        var nullableAttr = parameter
            .CustomAttributes
            .FirstOrDefault(a => a.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

        if (nullableAttr is { ConstructorArguments.Count: 1 })
        {
            var arg = nullableAttr.ConstructorArguments[0];

            if (arg.ArgumentType == typeof(byte) && (byte)arg.Value! == 2)
                return true;

            if (arg.Value is IReadOnlyCollection<CustomAttributeTypedArgument> args &&
                args.FirstOrDefault().Value is byte and 2)
                return true;
        }

        return false;
    }
}
