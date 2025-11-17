> 🧭 HubDocs is a developer-friendly UI tool like Swagger, but for **SignalR hubs** — auto-discover your hubs, explore methods, invoke calls, and preview live client messages.

https://hubdocs.mberrishdev.me/

[![GitHub stars](https://img.shields.io/github/stars/mberrishdev/HubDocs?style=social)](https://github.com/mberrishdev/HubDocs/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/mberrishdev/HubDocs?style=social)](https://github.com/mberrishdev/HubDocs/network)
[![Open issues](https://img.shields.io/github/issues/mberrishdev/HubDocs)](https://github.com/mberrishdev/HubDocs/issues)


# HubDocs

[![NuGet](https://img.shields.io/nuget/v/HubDocs.svg)](https://www.nuget.org/packages/HubDocs)
[![License](https://img.shields.io/github/license/mberrishdev/HubDocs)](LICENSE)

HubDocs is a powerful tool for exploring and documenting SignalR hubs in your ASP.NET Core applications. It provides a beautiful, Swagger-like UI that automatically discovers and displays all your SignalR hubs and their methods.

## Features

- 🔍 **Automatic Hub Discovery**: Automatically finds all SignalR hubs in your application
- 📝 **Method Documentation**: Shows method signatures, parameters, and return types
- 🎨 **Beautiful UI**: Swagger-inspired dark theme interface
- 🔌 **Easy Integration**: Simple setup with just a few lines of code
- 📦 **NuGet Package**: Easy to install and use in any ASP.NET Core project
- 📡 **Live Client Logging**: Displays real-time messages sent from server to clients via strongly-typed interfaces

## 🎥 Live Demo

![HubDocs Demo](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/demo.gif)

> The HubDocs UI in action — exploring hubs, invoking methods, and seeing real-time client logs.

---

## 🖼️ Screenshots

<p align="center">
  <img src="https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot1.png" alt="HubDocs Screenshot 1" width="800"/>
  <br/><em>📌 SignalR Hub list</em>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot2.png" alt="HubDocs Screenshot 2" width="800"/>
  <br/><em>🔍 Interactive method parameter inputs with \"Try it\" support</em>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot4.png" alt="HubDocs Screenshot 4" width="800"/>
  <br/><em>📡 Live client method logging with JSON preview</em>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot3.png" alt="HubDocs Screenshot 3" width="800"/>
  <br/><em>📭 No methods found — HubDocs will show helpful instructions if a hub is registered without a route.</em>
</p>

## 🔍 What You Get

- A dashboard of all registered hubs
- Parameter-aware “Try It” method testers
- Strongly-typed client method preview with JSON payloads
- Live server → client message tracking
- 
## Installation

```bash
dotnet add package HubDocs
```

## Quick Start

1. Add HubDocs to your ASP.NET Core application:

```csharp
using HubDocs;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Register your SignalR hubs with MapHub
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");

// Add HubDocs - discovers hubs marked with [HubDocs] attribute
app.AddHubDocs();

app.Run();
```

2. Mark your hubs with the `[HubDocs]` attribute:

```csharp
[HubDocs]
public class ChatHub : Hub<IChatClient>
{
    // ... your hub methods
}

[HubDocs]
public class NotificationHub : Hub
{
    // ... your hub methods
}
```

3. Access the HubDocs UI at `/hubdocs/index.html` or `/hubdocs/` in your browser.

## Example

```csharp
public interface IChatClient
{
    Task ReceiveMessage(string user, string message);
    Task Connected(string connectionId);
}

[HubDocs]  // Mark hub for documentation
public class ChatHub : Hub<IChatClient>
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.Connected(Context.ConnectionId);
    }
}
```

> **Note:** To fully leverage HubDocs, your hubs should implement `Hub<T>` with a strongly-typed client interface (`T`) that defines the client-callable methods. HubDocs will automatically extract and render both hub and client method metadata in the UI.

HubDocs will automatically discover marked hubs and display:
- The hub name and full type name
- The route where the hub is registered
- All public methods with their parameters and return types
- Client methods from the strongly-typed interface
- A beautiful, interactive UI to explore the hub

## Configuration

### Basic Usage

```csharp
// Register your hubs
app.MapHub<ChatHub>("/hubs/chat");

// Add HubDocs
app.AddHubDocs();
```

### Scanning Specific Assemblies

If your hubs are in external assemblies, you can specify them:

```csharp
app.AddHubDocs(typeof(ExternalHub).Assembly);
```

### Opt-in Documentation

Only hubs marked with `[HubDocs]` attribute will appear in the documentation UI. This gives you control over which hubs are publicly documented.

## Contributing

We welcome contributions! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Inspired by Swagger UI
- Built with ASP.NET Core
- Uses Tailwind CSS for styling

## Support

If you find a bug or have a feature request, please [open an issue](https://github.com/mberrishdev/HubDocs/issues).

## Authors

- Mikheil Berishvili - [mberrishdev](https://github.com/mberrishdev)

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request 
