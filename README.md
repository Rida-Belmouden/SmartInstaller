# SmartInstaller

> Modern open-source software package manager inspired by Ninite, built with ASP.NET Core, Clean Architecture, and CQRS.

![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)
![Build](https://img.shields.io/badge/Build-Passing-brightgreen)
![Tests](https://img.shields.io/badge/Tests-35%20Passing-success)

---

## Overview

SmartInstaller is a modern software installation platform that allows users to discover, install, and keep applications up to date through a clean web interface and a lightweight desktop agent.

The project is designed with scalability, maintainability, and testability in mind using:

- ASP.NET Core
- Entity Framework Core
- Clean Architecture
- CQRS
- SQLite / SQL Server
- xUnit Integration Tests

---

## Features

### Catalog

- Application catalog
- Categories
- Tags
- Publishers
- Search & filtering

### Application Versions

- Multiple versions per application
- Latest version management
- Release dates
- Version history

### Installer Profiles

- Multiple installers per version
- Architecture support
  - x86
  - x64
  - ARM64
- Installer type support
  - EXE
  - MSI
  - MSIX
  - ZIP
- Silent installation parameters

### API

Public API

- List applications
- Search applications
- Get application details
- List versions
- Installer profiles

Admin API

- Create applications
- Manage versions
- Manage installer profiles

---


## Agent API

The Agent API provides the desktop agent with a filtered software catalog, update detection, and secure installer metadata.

```http
GET  /api/agent/catalog?architecture=x64
POST /api/agent/check-updates
GET  /api/agent/installer-manifest/{installerProfileId}
```

The installer manifest includes the download URL, SHA-256 checksum, file size, architecture, installer type, and silent installation arguments.

## Desktop Agent

The Windows desktop agent can scan installed software from the standard uninstall registry locations:

- `HKLM` 64-bit applications
- `HKLM` 32-bit applications (`WOW6432Node` registry view)
- `HKCU` per-user applications

The scanner collects application name, version, publisher, installation location, uninstall command, and installation date. It filters hidden system components, normalizes application names, removes duplicate entries, supports cancellation, and displays the results in a searchable WPF interface.

Run the agent:

```bash
dotnet run --project src/SmartInstaller.Agent
```

> The desktop agent requires Windows.

## Project Structure

```text
src/
│
├── SmartInstaller.Api
├── SmartInstaller.Core
├── SmartInstaller.Data
├── SmartInstaller.Services
└── SmartInstaller.Agent

tests/

└── SmartInstaller.Tests
```

---

## Architecture

```
Presentation
        │
        ▼
Controllers
        │
        ▼
Application Services (CQRS)
        │
        ▼
Domain
        │
        ▼
Infrastructure (EF Core)
        │
        ▼
Database
```

---

## Technologies

| Technology | Version |
|------------|---------|
| .NET | 10 |
| ASP.NET Core | 10 |
| Entity Framework Core | 10 |
| SQLite | Latest |
| SQL Server | Supported |
| xUnit | Latest |

---

## Running

```bash
git clone https://github.com/Rida-Belmouden/SmartInstaller.git

cd SmartInstaller

dotnet restore

dotnet build

dotnet test

dotnet run --project src/SmartInstaller.Api
```

---

## Testing

Current status

```
Build ✔

Tests ✔

35 / 35 Passing
```

Run tests

```bash
dotnet test
```

---

## Roadmap

### Completed

- Clean Architecture
- CQRS
- Applications
- Categories
- Tags
- Versions
- Installer Profiles
- Agent API
- Windows installed-software scanner
- Integration Tests

### In Progress

- Agent API client
- Update matching engine
- Download Service

### Planned

- Authentication
- User Accounts
- Package Repository
- Digital Signature Verification
- Automatic Updates
- Web Dashboard

---

## Contributing

Contributions are welcome.

Please:

- Create a feature branch
- Write tests
- Keep the build passing
- Submit a Pull Request

---

## License

MIT License

---

## Author

**Rida Belmouden**

GitHub

https://github.com/Rida-Belmouden

---

⭐ If you like this project, consider giving it a star.

## Windows Agent

The Windows agent is split into a reusable core library and a WPF presentation layer:

```text
SmartInstaller.Agent.Core   Scanner, matching, API client, update synchronization
SmartInstaller.Agent        Windows WPF interface
```

Start the API first, then run the agent:

```bash
dotnet run --project src/SmartInstaller.Api
dotnet run --project src/SmartInstaller.Agent
```

The API URL is configured in `src/SmartInstaller.Agent/appsettings.json`.
The agent scans installed Windows software, matches supported applications against the SmartInstaller catalog, and displays available updates.
