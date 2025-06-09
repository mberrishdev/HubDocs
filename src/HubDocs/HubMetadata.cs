namespace HubDocs;

public class HubMetadata
{
    public string HubName { get; set; } = null!;
    public string HubFullName { get; set; } = null!;
    public string? Path { get; set; } = null!;
    public List<HubMethodMetadata> Methods { get; set; } = new();
}