namespace HubDocs;

public class HubMethodMetadata
{
    public string MethodName { get; set; } = default!;
    public List<string> ParameterTypes { get; set; } = new();
    public string ReturnType { get; set; } = default!;
}