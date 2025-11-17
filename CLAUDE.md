# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HubDocs is a .NET library that provides a Swagger-like UI for exploring and documenting SignalR hubs in ASP.NET Core applications. It's distributed as a NuGet package that automatically discovers SignalR hubs and their methods, displaying them in an interactive web interface.

## Solution Structure

- `src/HubDocs/` - Main library project that gets published to NuGet
- `samples/HubDocs.Sample/` - Sample ASP.NET Core app demonstrating HubDocs usage
- `tests/HubDocs.UnitTests/` - xUnit test project

## Key Architecture Components

### Hub Discovery System

The library uses attribute-based discovery with endpoint inspection:

1. **HubDocsAttribute** (`HubDocsAttribute.cs`) - Simple marker attribute applied to hub classes. No parameters needed - it just marks which hubs should be documented.

2. **GetHubRoutesFromEndpoints** (`Extensions.cs:51`) - Reads routes from registered SignalR endpoints by:
   - Accessing the `EndpointDataSource` from DI
   - Inspecting endpoint metadata for `Microsoft.AspNetCore.SignalR.HubMetadata`
   - Extracting hub types and their registered routes via reflection
   - Returns a dictionary mapping hub types to their routes

3. **DiscoverSignalRHubs Method** (`Extensions.cs:83`) - Private method that:
   - Accepts the hub routes dictionary from endpoint inspection
   - Scans assemblies for types inheriting from `Hub` with `[HubDocs]` attribute
   - Only includes hubs that are both attributed AND registered in endpoints
   - Extracts hub methods using `GetAllPublicHubMethods()` which walks the inheritance chain
   - Detects strongly-typed client interfaces (when hub extends `Hub<T>`)
   - Deduplicates methods by signature to handle inheritance correctly

### Metadata Model

- **HubMetadata** - Represents a single hub with its methods and optional client interface
- **HubMethodMetadata** - Represents a method with parameters and return type
- Both hub methods (server-callable) and client methods (client-callable via strongly-typed interface) use the same metadata structure

### API Endpoints

The `AddHubDocs()` extension creates three endpoints:
- `/hubdocs/hubdocs.json` - Returns JSON metadata of all discovered hubs by reading from the endpoint data source
- `/hubdocs/index.html` - Serves the embedded HTML UI from resources
- `/hubdocs` - Redirects to index.html

The HTML UI is embedded as a resource in the project (`wwwroot/hubdocs.html`).

### Usage Pattern

Users register hubs with standard SignalR registration, then mark them for documentation:

```csharp
// Mark hub for documentation
[HubDocs]
public class ChatHub : Hub<IChatClient> { }

// Register hub normally
app.MapHub<ChatHub>("/hubs/chat");

// Add HubDocs - discovers attributed hubs from registered endpoints
app.AddHubDocs();
```

## Building and Testing

### Build the solution
```bash
dotnet build HubDocs.sln
```

### Run all tests
```bash
dotnet test HubDocs.sln
```

### Run tests in a specific project
```bash
dotnet test tests/HubDocs.UnitTests/HubDocs.UnitTests.csproj
```

### Run a single test
```bash
dotnet test tests/HubDocs.UnitTests/HubDocs.UnitTests.csproj --filter "FullyQualifiedName~HubRouteRegistryTests.AddMapping_WhenCalled_ShouldAddMapping"
```

### Run the sample application
```bash
dotnet run --project samples/HubDocs.Sample/HubDocs.Sample.csproj
```

Then navigate to `/hubdocs` to see the UI.

## Package Publishing

The main project (`src/HubDocs/HubDocs.csproj`) is configured with `GeneratePackageOnBuild=true`. Building in Release mode automatically generates the NuGet package. Current version is defined in the csproj file.

## Important Implementation Details

### Endpoint Inspection Pattern
`GetHubRoutesFromEndpoints()` uses reflection to inspect SignalR's internal `HubMetadata` from the endpoint data source. This is necessary because SignalR doesn't expose hub routes through a public API. The method looks for metadata with the full name `"Microsoft.AspNetCore.SignalR.HubMetadata"` and extracts the `HubType` property.

### Attribute-Based Opt-In
The `[HubDocs]` attribute is a simple marker with no parameters. It serves as an opt-in mechanism - only hubs with this attribute appear in the documentation UI. Hubs must be both:
1. Marked with `[HubDocs]` attribute
2. Registered via `MapHub<T>()` in the endpoint data source

### Strongly-Typed Client Support
The discovery process checks if a hub inherits from `Hub<TClient>` (Extensions.cs:133-145). If so, it extracts the client interface type and its methods. This enables HubDocs to display both server methods (callable by clients) and client methods (callable by server).

### Method Signature Deduplication
`GetAllPublicHubMethods()` walks the inheritance chain and uses `GetMethodSignature()` to deduplicate overridden methods, ensuring each unique method appears only once in the metadata.

### Nullability Detection
The `IsNullable()` method (Extensions.cs:216) uses reflection to detect C# nullable reference types by checking for the `NullableAttribute`, which is critical for accurate parameter type display in the UI.
- to memorize