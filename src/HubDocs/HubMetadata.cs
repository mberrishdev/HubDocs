namespace HubDocs;

public class HubMetadata
{
    public string HubName { get; set; } = default!;
    public string HubFullName { get; set; } = default!;
    public List<HubMethodMetadata> Methods { get; set; } = new();
}