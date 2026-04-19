namespace HubDocs;

public class HubTypeSchemaMetadata
{
    public string Name { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Kind { get; init; } = null!;
    public List<string>? EnumValues { get; init; }
    public List<HubSchemaPropertyMetadata>? Properties { get; init; }
    public string Example { get; init; } = null!;
}

public class HubSchemaPropertyMetadata
{
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public bool IsNullable { get; init; }
    public string Example { get; init; } = null!;
}
