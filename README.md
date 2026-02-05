# XekuII Next Generation Framework

XekuII is a modern, AI-native web application framework built on **DevExpress XAF** and **.NET 8**. It streamlines backend development by leveraging YAML-driven entity definitions and automated code generation, allowing developers to focus on business logic while the framework handles the boilerplate.

[中文說明 (Traditional Chinese)](README.zh-Hant.md)

## Key Features

- **YAML-Driven Development**: Define your data models (Entities), validation rules, and relationships using simple, readable YAML files.
- **Automated Code Generation**: The `XekuII.Generator` tool automatically produces robust C# Business Objects (XPO) and ASP.NET Core Web API Controllers from your YAML definitions.
- **AI-Ready Architecture**: Designed with AI integration in mind, facilitating the development of intelligent applications.
- **Modern Tech Stack**: Built on the latest .NET 8 and DevExpress XAF Blazor/WebAPI technologies.

## Project Structure

- **`XekuII.ApiHost`**: The main ASP.NET Core backend application hosting the XAF Security System, Web API, and Business Objects.
- **`XekuII.Generator`**: The CLI tool responsible for parsing YAML definitions and generating C# code.
- **`XekuII.CLI`**: (Optional) Command-line interface for managing the XekuII environment.
- **`entities/`**: The recommended directory for storing your `.xeku.yaml` entity definition files.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [DevExpress Universal Subscription](https://www.devexpress.com/) (Required for XAF & XPO)
- A text editor or IDE (VS Code, Visual Studio 2022)

## Getting Started

### 1. Define Entities

Create a `.xeku.yaml` file in the `entities/` directory (e.g., `entities/Customer.xeku.yaml`):

```yaml
entity: Customer
icon: BO_Customer
fields:
  - name: Name
    type: string
    required: true
  - name: Email
    type: string
```

### 2. Generate Code

Run the generator using the `dotnet run` command from the root directory:

```powershell
# Syntax: dotnet run --project XekuII.Generator -- <entities-dir> [options]
dotnet run --project XekuII.Generator -- ./entities --output ./XekuII.ApiHost/BusinessObjects --controllers ./XekuII.ApiHost/API
```

### 3. Run the Application

Start the backend server:

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj
```

The API will be available at `http://localhost:5000` (or the port configured in `launchSettings.json`).

## Database Update

If you modify the database schema, you may need to update the database:

```powershell
dotnet run --project XekuII.ApiHost/XekuII.ApiHost.csproj -- --updateDatabase --forceUpdate --silent
```

## License

Private Project. All rights reserved.
