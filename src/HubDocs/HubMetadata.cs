namespace HubDocs;

public class HubMetadata
{
    public string HubName { get; init; } = null!;
    public string HubFullName { get; init; } = null!;
    public string? Path { get; init; } = null!;
    public List<HubMethodMetadata> Methods { get; init; } = [];

    public string? ClientInterfaceName { get; set; }
    public List<HubMethodMetadata>? ClientMethods { get; set; }
}