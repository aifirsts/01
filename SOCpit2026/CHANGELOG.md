# SOCpit Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Date-based Versioning](https://semver.org/spec/v2.0.0.html).

## [2026.10.15] - 2026-10-15

### Quality Hardening Release (test suite, repository layout, coverage gate)

- **Test suite: 5 -> 261 tests** across four xUnit projects plus a shared test harness:
  - `SOCpit.Core.Tests` - domain invariants, `ADR-V2-SEC-001` conservative security policy, graph validator (catalog/state/workspace integrity, dependency DAGs).
  - `SOCpit.Application.Tests` - all 8 application services against in-memory repository fakes (CRUD, audit trail, lock/unlock, incident resolution, evidence hashing).
  - `SOCpit.Storage.Tests` - all 8 EF Core repositories, `SOCpitDbContext` model (22 entities), JSON value converters and `UnitOfWork` against real in-memory SQLite.
  - `SOCpit.Import.Tests` - `MatrixMapper` parsing rules, MATRIX V3 importer, exporter and full export -> import round-trip.
  - `SOCpit.TestSupport` - shared fakes, SQLite in-memory database harness, deterministic entity builders and temp-directory helper.
- **Code coverage quality gate**: CI now collects `XPlat Code Coverage`, publishes a coverage summary and fails below `85%` line coverage. Current measured result: **3,204 / 3,419 lines = 93.71%**.
- **Repository layout aligned with the architecture documentation**: every repository interface (`SOCpit.Application/Repositories`) and implementation (`SOCpit.Storage/Repositories`) now lives in its own file.
- **Fixed**: audit-log date-range queries (`GetByDateRangeAsync`) could not be translated by the SQLite provider for `DateTimeOffset` comparisons; the window is now applied in memory with an explanatory comment.
- **Fixed**: read-only repository queries now consistently use `AsNoTracking()` (vulnerability, incident, evidence, host, user, network and audit repositories).
- **CI**: TRX test results, test discovery summary and coverage are published as build artifacts, job summaries and annotations; the coverage threshold is enforced as a quality gate.
- **Versioning**: assembly/file/product version bumped to `2026.10.15`.

## [2026.10.11] - 2026-10-11

### Production Release & Automated CI/CD Validation
- **GitHub Actions CI/CD**: Added `.github/workflows/build-and-release.yml` for automated build, test, and self-contained single-file EXE packaging on `windows-latest` (.NET 10.0).
- **Core Domain & Security Policy**: Implemented conservative `SecurityPolicy` (`ADR-V2-SEC-001`) and `GraphValidator` in `SOCpit.Core`.
- **Storage & Persistence**: Added EF Core 10.0 JSON value converters for collection/dictionary properties and automatic database initialization/seeding at `C:\SOCpit2026\socpit.db`.
- **WPF UI & MVVM**: Added View code-behind files, fixed XAML bindings/styles, generated valid Windows icon (`socpit.ico`), and updated application manifest.
- **Documentation & Audit**: Added `INSTALL.md` (RU/EN), updated `README.md` with release links, and published `audit/FINAL-VALIDATION-2026-10-11.md` (`12/12 PASS-1`, `COMPLETED_WITH_PASS_F`).

## [2026.10.09] - 2026-10-09

### Initial Release

This is the first release of SOCpit - Security Operations Center.

### Added

#### Core Features
- **Domain Models**: Complete domain model implementation
  - Threat, Vulnerability, SecurityIncident, Evidence
  - Host, Network, Subnet, Vlan, Service, Application
  - User, Permission, Role, Group, Session
  - SecurityControl, ComplianceCheck, AuditLog, LogEntry, Report, NetworkConnection
  - GraphValidator for security relationship validation

- **Application Layer**: CQRS and Service pattern implementation
  - Command and Query interfaces
  - Repository interfaces for all entities
  - Service implementations for all major entities
  - Unit of Work pattern for transaction management

- **Storage Layer**: Entity Framework Core with SQLite
  - SOCpitDbContext with all entity configurations
  - Repository implementations for all entities
  - Unit of Work implementation
  - Database migration support

- **Import/Export Layer**: MATRIX V3 integration
  - MatrixImporter for importing data from MATRIX V3
  - MatrixExporter for exporting data to MATRIX format
  - MatrixMapper for mapping between MATRIX and SOCpit models

- **Presentation Layer**: WPF UI with MVVM
  - MainWindow with navigation sidebar
  - Dashboard view with statistics and recent items
  - Threats view with filtering and CRUD operations
  - Vulnerabilities view with filtering and CRUD operations
  - Incidents view with filtering, CRUD, and resolve operations
  - Evidence view with filtering and CRUD operations
  - Hosts view with filtering and CRUD operations
  - Networks view with filtering and CRUD operations
  - Users view with filtering, CRUD, lock/unlock operations
  - Reports view with generation and export
  - Settings view with configuration and audit logs
  - Generic.xaml theme with styles and resources
  - Value converters for data binding

- **Testing**: Unit tests for domain models
  - xUnit test framework
  - Domain model validation tests
  - Creation and validation tests for all entities

#### Infrastructure
- **.NET 10**: Target framework with latest features
- **WPF**: Windows Presentation Foundation for UI
- **Entity Framework Core**: ORM for database access
- **SQLite**: Lightweight embedded database
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Logging**: Microsoft.Extensions.Logging

#### Project Structure
- `SOCpit.Core`: Domain layer with models and validation
- `SOCpit.Application`: Application layer with services and CQRS
- `SOCpit.Storage`: Infrastructure layer with EF Core and repositories
- `SOCpit.Import`: Integration layer with importers and exporters
- `SOCpit.App`: Presentation layer with WPF UI
- `SOCpit.Core.Tests`: Unit tests for domain layer

#### Documentation
- **README.md**: Complete project documentation
- **ARCHITECTURE.md**: Architecture and design decisions
- **SECURITY.md**: Security policies and practices
- **CHANGELOG.md**: This file
- **LICENSE**: MIT License

### Changed
- N/A (Initial release)

### Deprecated
- N/A (Initial release)

### Removed
- N/A (Initial release)

### Fixed
- N/A (Initial release)

### Security
- Implemented conservative security policy (ADR-V2-SEC-001)
- Role-based access control (RBAC)
- Data classification system
- Audit logging for all actions
- Session management
- Password policy enforcement
- Evidence integrity verification

## [Unreleased]

### Added
- REST API for remote access (planned)
- Real-time monitoring with SignalR (planned)
- Machine learning integration for anomaly detection (planned)
- Cloud deployment support (Azure, AWS) (planned)
- Docker container support (planned)

### Changed
- N/A

### Deprecated
- N/A

### Removed
- N/A

### Fixed
- N/A

### Security
- N/A

---

## Versioning

SOCpit uses **date-based versioning** in the format `YYYY.MM.DD`.

### Version Format
- `YYYY`: Year of release
- `MM`: Month of release (01-12)
- `DD`: Day of release (01-31)

### Version Examples
- `2026.10.09`: October 9, 2026
- `2026.12.25`: December 25, 2026
- `2027.01.15`: January 15, 2027

### Version Comparison
Versions are compared lexicographically:
- `2026.10.09` < `2026.10.10`
- `2026.10.31` < `2026.11.01`
- `2026.12.31` < `2027.01.01`

## Release Process

1. **Development**: Features and bug fixes are developed in feature branches
2. **Testing**: All changes are tested and validated
3. **Documentation**: Changes are documented in this changelog
4. **Review**: Code review and approval
5. **Merge**: Changes are merged to main branch
6. **Tag**: Release is tagged with version number
7. **Build**: Release build is created
8. **Package**: Release is packaged (ZIP, NuGet, etc.)
9. **Publish**: Release is published to GitHub and other channels
10. **Announce**: Release is announced to users

## Contributing

When contributing to SOCpit, please follow these guidelines:

1. **Changelog**: Update this file with your changes
2. **Versioning**: Follow the date-based versioning format
3. **Documentation**: Update relevant documentation
4. **Testing**: Add tests for new features and bug fixes
5. **Commits**: Use clear, descriptive commit messages
6. **Pull Requests**: Provide detailed description of changes

For more information, see the [Contributing Guide](CONTRIBUTING.md) (future).

## Support

For support, questions, or feedback:
- Open an issue on [GitHub](https://github.com/aifirsts/SOCpit2026/issues)
- Contact us at support@aifirsts.com (future)
- Join our [Discord community](https://discord.gg/example) (future)

---

**SOCpit - Your Comprehensive Security Operations Center**

Copyright (c) 2026 aifirsts
