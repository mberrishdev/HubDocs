namespace HubDocs;

public class HubMethodMetadata
{
    public string MethodName { get; init; } = null!;
    public List<string> ParameterTypes { get; init; } = [];
    public string ReturnType { get; init; } = null!;
}