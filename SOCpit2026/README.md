# SOCpit (Security Operations Center) v2026.10.11

**Version**: `2026.10.11`  
**Release Date**: `2026-10-11`  
**Status**: `COMPLETED_WITH_PASS_F` (Production Ready — 12/12 Quality Gates `PASS-1`)

SOCpit is a comprehensive Security Operations Center (SOC) desktop platform built with **.NET 10.0** and **WPF**, designed for security analysts and incident responders to manage threats, vulnerabilities, incidents, forensic evidence, hosts, networks, users, audit trails, and compliance reports.

---

## Download & Quick Start

### Latest Release: [SOCpit v2026.10.11 - Production Ready](https://github.com/aifirsts/01/releases/tag/v2026.10.11)

- **Release Archive**: [`SOCpit-2026.10.11-RELEASE.zip`](https://github.com/aifirsts/01/releases/download/v2026.10.11/SOCpit-2026.10.11-RELEASE.zip)
- **All Releases**: [https://github.com/aifirsts/01/releases](https://github.com/aifirsts/01/releases)
- **Installation Guide**: See [`INSTALL.md`](INSTALL.md) for step-by-step instructions (RU / EN).

### Quick Run (Windows x64)
1. Download [`SOCpit-2026.10.11-RELEASE.zip`](https://github.com/aifirsts/01/releases/download/v2026.10.11/SOCpit-2026.10.11-RELEASE.zip).
2. Extract the archive to any directory.
3. Run `SOCpit.App.exe` (self-contained single-file executable — no separate .NET installation required).
4. Data and logs are stored automatically in `C:\SOCpit2026` (`C:\SOCpit2026\socpit.db` and `C:\SOCpit2026\logs\`).

---

## Features

- **Dashboard**: Real-time overview of security posture, threat metrics, open/resolved incidents, and active hosts.
- **Threat Management**: Track, analyze, and mitigate security threats with MITRE ATT&CK tactic/technique mapping.
- **Vulnerability Management**: Monitor CVEs, CVSS v3.1 scores/vectors, exploit availability, and patch status.
- **Incident Response**: Manage security incidents from detection through containment, eradication, and resolution.
- **Evidence Collection**: Securely store, classify, and verify digital forensic evidence with SHA-256 integrity hashing.
- **Asset Management**: Inventory and monitor hosts, servers, workstations, IoT devices, and network segments/VLANs.
- **User & Role Management**: Role-based access control (Administrator, Analyst, Operator, Auditor, ReadOnly) with audit logging.
- **Reporting & Import/Export**: Generate executive and compliance security reports; import/export JSON, CSV, and XML data.
- **Conservative Security Policy (`ADR-V2-SEC-001`)**: Strict node-level and link-level security classification (`Public`, `Internal`, `Restricted`, `Confidential`, `Secret`).

---

## Architecture (5-Layer Clean Architecture)

1. **`SOCpit.Core`**: Domain models, immutable records, conservative `SecurityPolicy` (`ADR-V2-SEC-001`), and `GraphValidator`.
2. **`SOCpit.Application`**: Application services, CQRS commands/queries, DTOs, and repository/import/export interfaces.
3. **`SOCpit.Storage`**: Entity Framework Core 10.0 SQLite persistence (`C:\SOCpit2026\socpit.db`) and repository implementations.
4. **`SOCpit.Import`**: Multi-format data importers and exporters (JSON, CSV, XML).
5. **`SOCpit.App`**: WPF desktop client using MVVM (`CommunityToolkit.Mvvm`), Dark SOC theme, and `Serilog` logging.

---

## Building from Source

### Prerequisites
- Windows 10/11 x64
- .NET 10.0 SDK (`10.0.100` or newer)

### Build, Test & Publish
```bash
dotnet restore SOCpit2026/SOCpit.sln
dotnet build SOCpit2026/SOCpit.sln --configuration Release
dotnet test SOCpit2026/SOCpit.sln --configuration Release --no-build
dotnet publish SOCpit2026/src/SOCpit.App/SOCpit.App.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## Audit & Validation
- Final Validation Report: [`audit/FINAL-VALIDATION-2026-10-11.md`](audit/FINAL-VALIDATION-2026-10-11.md)
- Audit Status: [`audit/STATUS.json`](audit/STATUS.json)

## License
Proprietary — All rights reserved.
