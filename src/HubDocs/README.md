# HubDocs

A Swagger-like UI for exploring and documenting SignalR hubs in ASP.NET Core applications.

## Features

- 🔍 Automatic discovery of SignalR hubs
- 📝 Method documentation with parameters and return types
- 🎨 Beautiful Swagger-inspired dark theme UI
- 🔌 Easy integration with minimal configuration

## Screenshots

![Screenshot](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot1.png)
![Screenshot](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot2.png)
![Screenshot](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot3.png)

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

//Configure and register hub
app.MapHubAndRegister<YourHub>("/hub");

// Configure HubDocs middleware
app.AddHubDocs();

// ... your other middleware configurations ...
```

2. Access the HubDocs UI at `/hubdocs/index.html` or `/hubdocs/` in your browser.

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

Register hubs:

```csharp
app.MapHubAndRegister<ChatHub>("/hubs/chat");

app.AddHubDocs();
```

## Links

- [GitHub Repository](https://github.com/mberrishdev/HubDocs)
- [Documentation](https://github.com/mberrishdev/HubDocs#readme)
- [Report Issues](https://github.com/mberrishdev/HubDocs/issues)
