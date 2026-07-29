# SmartInstaller

> Modern open-source software package manager inspired by Ninite, built with ASP.NET Core, Clean Architecture, and CQRS.

![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/License-MIT-green)
![Build](https://img.shields.io/badge/Build-Passing-brightgreen)
![Tests](https://img.shields.io/badge/Tests-28%20Passing-success)

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

28 / 28 Passing
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
- Integration Tests

### In Progress

- Desktop Agent
- Download Service
- Update Detection

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
