namespace HubDocs;

public class HubDocsDocumentOptions
{
    public string Title { get; set; } = "HubDocs SignalR Protocol";
    public string Version { get; set; } = "1.0.0";
    public string? Description { get; set; } = "HubDocs protocol export with channels, messages, and schemas.";
    public string? TermsOfService { get; set; }
    public string? ProjectUrl { get; set; }
    public HubDocsContactOptions Contact { get; set; } = new();
    public HubDocsLicenseOptions License { get; set; } = new();
}

public class HubDocsContactOptions
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
}

public class HubDocsLicenseOptions
{
    public string? Name { get; set; }
    public string? Url { get; set; }
}
