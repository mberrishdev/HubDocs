# HubDocs

A Swagger-like UI for exploring and documenting SignalR hubs in ASP.NET Core applications.

## Features

- 🔍 Automatic discovery of SignalR hubs
- 📝 Method documentation with parameters and return types
- 🎨 Beautiful Swagger-inspired dark theme UI
- 🔌 Easy integration with minimal configuration

## Screenshots

![HubDocs UI](https://i.ibb.co/84LpzNFv/Screenshot-2025-06-06-at-11-08-50.png)
![HubDocs UI](https://i.ibb.co/k2BpY2Hh/Screenshot-2025-06-06-at-11-09-01.png)


*HubDocs UI showing SignalR hubs and their methods*

## Installation

```bash
dotnet add package HubDocs
```

## Quick Start

1. Add HubDocs to your ASP.NET Core application:

```csharp
using HubDocs;

var builder = WebApplication.CreateBuilder(args);
// ... your other configurations ...

var app = builder.Build();

// Configure HubDocs middleware
app.AddHubDocs(typeof(YourHub).Assembly);

// ... your other middleware configurations ...
```

2. Access the HubDocs UI at `/hubdocs/index.html`

## Example

```csharp
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
```

HubDocs will automatically discover and display:
- Hub name and full type name
- All public methods with parameters and return types
- Interactive UI for exploring the hub

## Configuration

Scan specific assemblies for hubs:

```csharp
app.AddHubDocs(
    typeof(Hub1).Assembly,
    typeof(Hub2).Assembly
);
```

## Links

- [GitHub Repository](https://github.com/mberrishdev/HubDocs)
- [Documentation](https://github.com/mberrishdev/HubDocs#readme)
- [Report Issues](https://github.com/mberrishdev/HubDocs/issues)
