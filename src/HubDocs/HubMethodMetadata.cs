namespace HubDocs;

public class HubMethodMetadata
{
    public string MethodName { get; init; } = null!;
    public string Signature { get; init; } = null!;
    public List<string> ParameterTypes { get; init; } = [];
    public List<HubParameterMetadata> Parameters { get; init; } = [];
    public string ReturnType { get; init; } = null!;
    public string? ExampleInvocation { get; init; }
    public string? ReturnExample { get; init; }
}

public class HubParameterMetadata
{
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public bool IsNullable { get; init; }
    public string Example { get; init; } = null!;
}