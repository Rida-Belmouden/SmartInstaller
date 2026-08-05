# SmartInstaller

> An open-source Windows software catalog and update-detection platform built with .NET 10, ASP.NET Core, Entity Framework Core, WPF, Clean Architecture, and CQRS-style application services.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Agent-Windows-0078D4)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-2EA44F)](LICENSE)
![Status](https://img.shields.io/badge/Status-Active%20Development-orange)

## Overview

SmartInstaller is a multi-project .NET solution for maintaining a curated software catalog and detecting updates for applications installed on Windows.

The current implementation provides:

- a public catalog API;
- administrative APIs for application versions and installer profiles;
- a browser-based catalog administration dashboard;
- a Windows WPF agent that scans installed software from the Registry;
- name normalization and catalog matching;
- update detection based on installed and latest catalog versions;
- installer manifests containing download and silent-install metadata;
- concurrent, resumable, and verified downloads;
- per-item pause, resume, and cancellation controls;
- silent installation with post-install verification;
- automated unit and integration tests.

SmartInstaller is inspired by the simplicity of tools such as Ninite, while being designed as an extensible learning and product-development project.

## Current capabilities

### Software catalog

- Seeded application catalog with categories, publishers, platforms, and tags.
- Search and filtering by text, category, and tag.
- Application details and version history.
- Multiple versions per application.
- Latest-version management.

### Installer profiles

Each application version can expose one or more installer profiles with:

- installer type: `EXE`, `MSI`, `MSIX`, or `ZIP`;
- architecture: `x86`, `x64`, `ARM64`, or `Any`;
- download URL;
- optional SHA-256 checksum;
- optional file size;
- silent install and uninstall arguments;
- administrator requirement;
- portable-package flag;
- enabled/disabled state.

Duplicate profiles for the same version, installer type, and architecture are prevented at both the service and database levels.

### Agent API

The desktop agent communicates with these endpoints:

```http
GET  /api/agent/catalog?architecture=x64
POST /api/agent/check-updates
GET  /api/agent/installer-manifest/{installerProfileId}
```

The Agent API:

- filters installers by system architecture;
- returns the latest active application versions;
- compares installed versions with catalog versions;
- returns compatible installer metadata;
- hides inactive or disabled installer profiles.

### Windows desktop agent

The WPF agent currently supports:

- scanning installed applications from standard Windows uninstall Registry locations;
- reading 32-bit, 64-bit, machine-wide, and per-user entries;
- collecting application name, version, publisher, install location, uninstall command, and install date;
- filtering hidden system components;
- removing duplicate Registry entries;
- searching the scanned application list;
- detecting the system architecture;
- normalizing application names such as `7-Zip 26.01 (x64)` to match catalog entries such as `7-Zip`;
- connecting to the SmartInstaller API;
- displaying matched applications and available updates;
- downloading multiple updates concurrently;
- resuming partial downloads and reusing verified cached files;
- retrying transient failures and verifying SHA-256 checksums;
- pausing, resuming, or cancelling individual downloads;
- displaying queue position, per-item speed, ETA, and live queue statistics;
- silently installing selected updates and verifying the installed version;
- canceling long-running scan and synchronization operations.

### Admin catalog dashboard

The ASP.NET Core MVC application includes an administration dashboard at:

```text
/admin/catalog
```

The current dashboard can:

- browse and search seeded applications;
- view application versions and installer profiles;
- create an application version;
- create an installer profile;
- deactivate an installer profile.

Creating new applications, categories, publishers, and tags from the dashboard is not implemented yet.

## Solution structure

```text
SmartInstaller/
├── src/
│   ├── SmartInstaller.Core/        Domain entities and shared base types
│   ├── SmartInstaller.Data/        EF Core context, mappings, migrations, and seed data
│   ├── SmartInstaller.Services/    Commands, queries, DTOs, validation, and handlers
│   ├── SmartInstaller.Api/         Public, admin, and agent HTTP APIs
│   ├── SmartInstaller.Web/         ASP.NET Core MVC admin dashboard
│   ├── SmartInstaller.Agent.Core/  Scanner, matching, API client, and synchronization logic
│   └── SmartInstaller.Agent/       Windows WPF desktop interface
├── tests/
│   └── SmartInstaller.Tests/       Agent unit tests and API integration tests
├── SmartInstaller.slnx
├── LICENSE
└── README.md
```

## Architecture

```text
┌──────────────────────────┐
│ SmartInstaller.Web       │
│ SmartInstaller.Agent     │
│ SmartInstaller.Api       │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│ Application Services     │
│ Commands / Queries / DTOs│
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│ Core Domain              │
│ Catalog / Installer      │
└────────────┬─────────────┘
             │
             ▼
┌──────────────────────────┐
│ Data Access              │
│ EF Core / SQL Server     │
└──────────────────────────┘
```

The Windows-specific scanner and synchronization workflow are isolated in `SmartInstaller.Agent.Core`, while the WPF project is responsible for presentation and user interaction.

## Technology stack

| Area | Technology |
|---|---|
| Runtime | .NET 10 |
| API | ASP.NET Core 10 |
| Admin UI | ASP.NET Core MVC + Bootstrap |
| Desktop UI | WPF |
| Persistence | Entity Framework Core 10 |
| Development database | SQL Server LocalDB |
| Integration-test database | SQLite in-memory |
| Testing | xUnit + `WebApplicationFactory` |
| Coverage collection | Coverlet |

## Prerequisites

- Windows 10 or Windows 11 for the desktop agent.
- .NET 10 SDK.
- SQL Server LocalDB or another SQL Server instance.
- Git.

The default development connection string uses:

```text
Server=(localdb)\MSSQLLocalDB;Database=SmartInstallerDb;Trusted_Connection=True;TrustServerCertificate=True;
```

It is defined in:

```text
src/SmartInstaller.Api/appsettings.Development.json
```

## Getting started

### 1. Clone the repository

```bash
git clone https://github.com/Rida-Belmouden/SmartInstaller.git
cd SmartInstaller
```

### 2. Restore and build

```bash
dotnet restore
dotnet build SmartInstaller.slnx
```

### 3. Apply the database migrations

```bash
dotnet ef database update \
  --project src/SmartInstaller.Data \
  --startup-project src/SmartInstaller.Api \
  --context ApplicationDbContext
```

On PowerShell, the same command can be written as:

```powershell
dotnet ef database update `
  --project src/SmartInstaller.Data `
  --startup-project src/SmartInstaller.Api `
  --context ApplicationDbContext
```

### 4. Start the API

```bash
dotnet run --project src/SmartInstaller.Api
```

Default development URLs:

```text
http://localhost:5272
https://localhost:7149
```

OpenAPI is available in Development at:

```text
http://localhost:5272/openapi/v1.json
```

### 5. Start the admin dashboard

In a second terminal:

```bash
dotnet run --project src/SmartInstaller.Web
```

Then open:

```text
http://localhost:5267/admin/catalog
```

The Web project calls the API URL configured in:

```text
src/SmartInstaller.Web/appsettings.json
```

### 6. Start the Windows agent

Keep the API running, then start the agent in another terminal:

```bash
dotnet run --project src/SmartInstaller.Agent
```

The agent API address is configured in:

```text
src/SmartInstaller.Agent/appsettings.json
```

Default configuration:

```json
{
  "Agent": {
    "ApiBaseUrl": "http://localhost:5272",
    "RequestTimeout": "00:00:30"
  }
}
```

In the agent:

1. Select **Scan applications**.
2. Select **Check updates**.
3. Open the **Updates** tab.

An application will only appear in update results when the catalog contains an active latest version and a compatible enabled installer profile.

## API summary

### Public catalog

```http
GET /api/applications
GET /api/applications/{publicId}
GET /api/applications/{publicId}/versions
GET /api/versions/{publicId}
GET /api/categories
GET /api/platforms
GET /api/tags
GET /api/installer-profiles
GET /api/installer-profiles/{publicId}
```

### Administrative operations

```http
POST   /api/admin/applications/{publicId}/versions
PUT    /api/admin/versions/{publicId}
PATCH  /api/admin/versions/{publicId}/set-latest
DELETE /api/admin/versions/{publicId}

POST   /api/admin/installer-profiles
PUT    /api/admin/installer-profiles/{publicId}
DELETE /api/admin/installer-profiles/{publicId}
```

### Agent operations

```http
GET  /api/agent/catalog
POST /api/agent/check-updates
GET  /api/agent/installer-manifest/{installerProfileId}
```

## Testing

Run the complete test suite:

```bash
dotnet test SmartInstaller.slnx
```

The repository currently defines **132 xUnit test cases**, covering:

- public catalog queries and filters;
- application-version creation, update, latest selection, and deletion;
- installer-profile creation, validation, filtering, update, and deactivation;
- agent catalog architecture filtering;
- update detection;
- installer-manifest retrieval;
- application-name normalization;
- installed-application matching and duplicate handling;
- HTTP downloads, retry policies, caching, and SHA-256 verification;
- resumable range downloads;
- concurrent queue execution and per-item controls;
- download-session telemetry;
- silent installation and post-install verification.

The API integration tests run against an isolated SQLite in-memory database through `WebApplicationFactory`.

## Database migrations

The repository includes migrations for:

- the initial catalog schema;
- expanded catalog entities;
- catalog schema rebuilding;
- seeded application catalog data;
- unique application-version constraints;
- installer-profile feature flags.

Create a new migration with:

```bash
dotnet ef migrations add MigrationName \
  --project src/SmartInstaller.Data \
  --startup-project src/SmartInstaller.Api \
  --context ApplicationDbContext \
  --output-dir Migrations
```

## Project status

### Implemented

- Catalog domain and database model.
- Seeded applications, categories, publishers, platforms, and tags.
- Application-version lifecycle APIs.
- Installer-profile lifecycle APIs.
- Agent catalog, update-check, and installer-manifest APIs.
- Windows installed-software scanner.
- Name normalization and catalog matching.
- End-to-end update detection in the WPF agent.
- Concurrent and resumable download engine.
- Retry, cache, and SHA-256 verification.
- Per-item pause, resume, and cancellation.
- Queue positions, speed, ETA, and live download statistics.
- Silent installation and installed-version verification.
- Admin catalog dashboard for versions and installer profiles.
- Unit and integration tests.

### Next milestones

- Background update service and tray integration.
- Scheduling and automatic update policies.
- Persistent update execution history and result reporting.
- Authentication and authorization for admin endpoints.
- Full CRUD for applications and catalog reference data.
- Device inventory and update history.
- CI workflow and automated release packaging.

## Important limitations

SmartInstaller is currently under active development:

- admin endpoints are not protected by authentication;
- the dashboard manages versions and installer profiles for existing seeded applications only;
- package metadata and download links must be curated by an administrator;
- downloads and installations currently run while the WPF agent is open;
- production deployment, signing, and hardened security configuration are not yet provided.

## Contributing

Contributions are welcome.

1. Create a feature branch:

   ```bash
   git checkout -b feature/your-feature
   ```

2. Keep the solution building and add or update tests.
3. Use clear commit messages.
4. Open a pull request against `main`.

## License

SmartInstaller is released under the [MIT License](LICENSE).

## Author

**Rida Belmouden**

- GitHub: [Rida-Belmouden](https://github.com/Rida-Belmouden)

---

If you find the project useful, consider starring the repository.
