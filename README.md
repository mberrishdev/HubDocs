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

![Screenshot](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot1.png)
![Screenshot](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot2.png)
![Screenshot](https://raw.githubusercontent.com/mberrishdev/HubDocs/main/docs/screenshots/screenshot3.png)

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
app.AddHubDocs(typeof(ChatHub).Assembly, typeof(NotificationHub).Assembly);

// ... your other middleware configurations ...
```

2. Access the HubDocs UI at `/hubdocs/index.html`

## Example

Here's a simple example of how HubDocs displays your SignalR hubs:

```csharp
public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }
}
```

HubDocs will automatically discover this hub and display:
- The hub name and full type name
- All public methods with their parameters and return types
- A beautiful, interactive UI to explore the hub

## Configuration

HubDocs is designed to work out of the box with minimal configuration. However, you can customize it by passing specific assemblies to scan:

```csharp
// Scan specific assemblies
app.ConfigureMiddleware(
    typeof(Hub1).Assembly,
    typeof(Hub2).Assembly
);
```

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

## Roadmap

- [ ] Add support for method descriptions via XML comments
- [ ] Add support for parameter descriptions
- [ ] Add support for hub descriptions
- [ ] Add support for custom themes
- [ ] Add support for hub grouping
- [ ] Add support for hub versioning

## Authors

- Mikheil Berishvili - [mberrishdev](https://github.com/mberrishdev)

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request 