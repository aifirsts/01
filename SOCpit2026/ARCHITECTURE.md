# SOCpit Architecture

## Overview

SOCpit is built using a **Clean Architecture** approach with **Domain-Driven Design (DDD)** principles. The application follows **SOLID** principles and implements **CQRS** (Command Query Responsibility Segregation) pattern with **Repository** and **Unit of Work** patterns for data access.

## Layered Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│              (SOCpit.App - WPF Application)                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   Views     │  │ ViewModels  │  │  Resources  │     │
│  │   (XAML)    │  │   (MVVM)    │  │   (Styles)  │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
                     ↑ Dependency Injection
                     ↓
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                           │
│             (SOCpit.Application)                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │  Commands   │  │   Queries   │  │  Services   │     │
│  │ (CQRS Write)│  │ (CQRS Read) │  │ (Business)  │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
                     ↑
                     ↓
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                              │
│                  (SOCpit.Core)                               │
│  ┌─────────────────────────────────────────────────────┐  │
│  │                 Domain Models                          │  │
│  │  • Entities (Threat, Vulnerability, Incident, etc.)   │  │
│  │  • Value Objects                                       │  │
│  │  • Enums (Severity, Status, Type, etc.)               │  │
│  │  • Domain Services (GraphValidator, etc.)            │  │
│  └─────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                     ↑
                     ↓
┌─────────────────────────────────────────────────────────┐
│                  Infrastructure Layer                       │
│  ┌─────────────────────┐  ┌─────────────────────┐        │
│  │   SOCpit.Storage     │  │   SOCpit.Import      │        │
│  │  • DbContext        │  │  • Importers         │        │
│  │  • Repositories     │  │  • Exporters         │        │
│  │  • UnitOfWork       │  │  • Mappers           │        │
│  │  • Migrations       │  │                     │        │
│  └─────────────────────┘  └─────────────────────┘        │
└─────────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────┐
│                  External Dependencies                       │
│  • Entity Framework Core                                  │
│  • SQLite                                               │
│  • .NET 10 Runtime                                      │
│  • WPF (Windows Presentation Foundation)                 │
└─────────────────────────────────────────────────────────┘
```

## Project Structure

### 1. SOCpit.Core (Domain Layer)

**Purpose**: Contains the core domain models, business logic, and domain services.

**Key Files**:
- `DomainModels.cs` - Infrastructure and base models (Host, Network, Service, Application)
- `SecurityModels.cs` - Security-specific models (Threat, Vulnerability, SecurityIncident, Evidence, SecurityControl, ComplianceCheck)
- `EvidenceModels.cs` - Evidence and audit models (Evidence, AuditLog, LogEntry, Report, NetworkConnection)
- `UserModels.cs` - User and identity models (User, Permission, Role, Group, Session, UserAuditEntry)
- `GraphValidator.cs` - Domain service for graph validation

**Design Patterns**:
- **Domain Model**: Rich domain objects with behavior and validation
- **Value Objects**: Immutable objects (enums, IDs)
- **Aggregate Root**: Threat, Vulnerability, Incident, Host, Network, User
- **Domain Services**: GraphValidator for complex domain logic

### 2. SOCpit.Application (Application Layer)

**Purpose**: Contains application-specific business logic, CQRS handlers, and services.

**Key Directories**:
- `Commands/` - Command handlers (write operations)
  - `ThreatCommands.cs` - Create, Update, Delete threat commands
- `Queries/` - Query handlers (read operations)
- `Services/` - Application services
  - `ThreatService.cs` - Threat management service
  - `VulnerabilityService.cs` - Vulnerability management service
  - `IncidentService.cs` - Incident management service
  - `EvidenceService.cs` - Evidence management service
  - `HostService.cs` - Host management service
  - `NetworkService.cs` - Network management service
  - `UserService.cs` - User management service
  - `AuditService.cs` - Audit logging service
- `Repositories/` - Repository interfaces
  - `IThreatRepository.cs` - Interface for threat repository
  - `IVulnerabilityRepository.cs` - Interface for vulnerability repository
  - `IIncidentRepository.cs` - Interface for incident repository
  - `IEvidenceRepository.cs` - Interface for evidence repository
  - `IHostRepository.cs` - Interface for host repository
  - `INetworkRepository.cs` - Interface for network repository
  - `IUserRepository.cs` - Interface for user repository
  - `IAuditLogRepository.cs` - Interface for audit log repository
  - `IUnitOfWork.cs` - Unit of work interface

**Design Patterns**:
- **CQRS (Command Query Responsibility Segregation)**: Separate read and write operations
- **MediatR Pattern**: Command and query handling (simplified implementation)
- **Service Pattern**: Business logic in service classes
- **Repository Pattern**: Abstract data access behind interfaces
- **Unit of Work**: Coordinate multiple repository operations

### 3. SOCpit.Storage (Infrastructure Layer)

**Purpose**: Implements data persistence using Entity Framework Core and SQLite.

**Key Files**:
- `SOCpitDbContext.cs` - Entity Framework Core database context
- `UnitOfWork.cs` - Unit of work implementation
- `Repositories/` - Repository implementations
  - `BaseRepository.cs` - Base repository with common operations
  - `ThreatRepository.cs` - Threat repository implementation
  - `VulnerabilityRepository.cs` - Vulnerability repository implementation
  - `IncidentRepository.cs` - Incident repository implementation
  - `EvidenceRepository.cs` - Evidence repository implementation
  - `HostRepository.cs` - Host repository implementation
  - `NetworkRepository.cs` - Network repository implementation
  - `UserRepository.cs` - User repository implementation
  - `AuditLogRepository.cs` - Audit log repository implementation

**Design Patterns**:
- **Repository Pattern**: Concrete implementations of repository interfaces
- **Unit of Work Pattern**: Coordinate database transactions
- **Entity Framework Core**: ORM for database access

### 4. SOCpit.Import (Integration Layer)

**Purpose**: Handles data import/export from/to external systems (MATRIX V3).

**Key Directories**:
- `Importers/` - Data importers
  - `MatrixImporter.cs` - Import from MATRIX V3
- `Exporters/` - Data exporters
  - `MatrixExporter.cs` - Export to MATRIX format
- `Mappers/` - Data mappers
  - `MatrixMapper.cs` - Map between MATRIX and SOCpit models

**Design Patterns**:
- **Adapter Pattern**: Adapt external data formats to domain models
- **Mapper Pattern**: Map between different data structures
- **Strategy Pattern**: Different import/export strategies

### 5. SOCpit.App (Presentation Layer)

**Purpose**: WPF application with MVVM pattern for user interface.

**Key Directories**:
- `Views/` - WPF views (XAML files)
  - `DashboardView.xaml` - Main dashboard
  - `ThreatsView.xaml` - Threat management view
  - `VulnerabilitiesView.xaml` - Vulnerability management view
  - `IncidentsView.xaml` - Incident management view
  - `EvidenceView.xaml` - Evidence management view
  - `HostsView.xaml` - Host management view
  - `NetworksView.xaml` - Network management view
  - `UsersView.xaml` - User management view
  - `ReportsView.xaml` - Reporting view
  - `SettingsView.xaml` - Settings view
- `ViewModels/` - WPF view models
  - `MainViewModel.cs` - Main view model with navigation
  - `DashboardViewModel.cs` - Dashboard view model
  - `ThreatsViewModel.cs` - Threat management view model
  - `VulnerabilitiesViewModel.cs` - Vulnerability management view model
  - `IncidentsViewModel.cs` - Incident management view model
  - `EvidenceViewModel.cs` - Evidence management view model
  - `HostsViewModel.cs` - Host management view model
  - `NetworksViewModel.cs` - Network management view model
  - `UsersViewModel.cs` - User management view model
  - `ReportsViewModel.cs` - Reporting view model
  - `SettingsViewModel.cs` - Settings view model
- `Themes/` - XAML themes and styles
  - `Generic.xaml` - Application-wide styles and resources
- `Converters/` - Value converters for data binding
  - `BooleanToInverseVisibilityConverter.cs` - Boolean to visibility converter
  - `BooleanToStatusConverter.cs` - Boolean to status string converter
  - `SeverityToColorConverter.cs` - Severity to color converters
  - `SubtractConverter.cs` - Multi-value converter for calculations

**Design Patterns**:
- **MVVM (Model-View-ViewModel)**: Separation of concerns in UI
- **Data Binding**: WPF data binding for UI updates
- **Command Pattern**: ICommand for action handling
- **Observer Pattern**: INotifyPropertyChanged for UI notifications
- **Template Method**: Base view model with common functionality

## Data Flow

### Read Operations (Query)
```
User Action → View → ViewModel → Query → Service → Repository → DbContext → Database
```

### Write Operations (Command)
```
User Action → View → ViewModel → Command → Service → Repository → UnitOfWork → DbContext → Database
```

### Import Operations
```
External Data → Importer → Mapper → Domain Models → Service → Repository → Database
```

### Export Operations
```
Database → Repository → Service → Mapper → Exporter → External Format
```

## Dependency Graph

```
SOCpit.App
├── SOCpit.Application
│   ├── SOCpit.Core
│   └── SOCpit.Storage
│       └── SOCpit.Core
├── SOCpit.Import
│   ├── SOCpit.Core
│   └── SOCpit.Storage
└── SOCpit.Core
```

## Technology Stack

### Core Technologies
- **.NET 10**: Latest .NET runtime
- **C# 10**: Latest C# language features
- **WPF (Windows Presentation Foundation)**: Desktop UI framework
- **Entity Framework Core**: Object-Relational Mapper
- **SQLite**: Lightweight embedded database

### Development Tools
- **Visual Studio 2022**: IDE (recommended)
- **JetBrains Rider**: Alternative IDE
- **VS Code**: Lightweight code editor with C# extension
- **dotnet CLI**: Command-line interface

### Testing
- **xUnit**: Unit testing framework
- **Microsoft.NET.Test.Sdk**: Test SDK
- **coverlet**: Code coverage

### Build & Deployment
- **MSBuild**: Build system
- **dotnet publish**: Deployment tool
- **Single-file publishing**: Self-contained deployment

## Architecture Decisions (ADRs)

### ADR-001: .NET 10 Target
**Status**: Accepted
**Decision**: Use .NET 10 as the target framework for the latest features and performance improvements.
**Consequences**: 
- Requires .NET 10 SDK for development
- Requires .NET 10 runtime for execution
- Enables use of latest C# features
- Provides better performance

### ADR-002: WPF for UI
**Status**: Accepted
**Decision**: Use WPF (Windows Presentation Foundation) for the user interface.
**Consequences**:
- Windows-only application
- Rich UI capabilities with XAML
- MVVM pattern support
- Data binding and styling support

### ADR-003: Clean Architecture
**Status**: Accepted
**Decision**: Implement Clean Architecture with separated layers.
**Consequences**:
- Clear separation of concerns
- Easier testing and maintenance
- Flexibility to change implementations
- Better scalability

### ADR-004: CQRS Pattern
**Status**: Accepted
**Decision**: Implement CQRS (Command Query Responsibility Segregation) pattern.
**Consequences**:
- Separate read and write operations
- Better performance for queries
- Clearer code organization
- Easier to scale read/write independently

### ADR-005: Repository Pattern
**Status**: Accepted
**Decision**: Use Repository pattern for data access.
**Consequences**:
- Abstract database access
- Easier to test
- Flexibility to change database
- Consistent data access interface

### ADR-006: Entity Framework Core
**Status**: Accepted
**Decision**: Use Entity Framework Core as ORM.
**Consequences**:
- Rapid development with code-first approach
- Support for multiple databases
- LINQ for querying
- Change tracking and migrations

### ADR-007: SQLite Database
**Status**: Accepted
**Decision**: Use SQLite as the default database.
**Consequences**:
- Zero-configuration deployment
- Single-file database
- Embedded, no server required
- Good performance for small to medium datasets
- Easy to backup and restore

### ADR-008: MVVM Pattern
**Status**: Accepted
**Decision**: Use MVVM (Model-View-ViewModel) pattern for WPF UI.
**Consequences**:
- Separation of UI and business logic
- Testable UI logic
- Data binding support
- Better maintainability

### ADR-009: Dependency Injection
**Status**: Accepted
**Decision**: Use Microsoft.Extensions.DependencyInjection for DI.
**Consequences**:
- Loose coupling between components
- Easier testing with mock dependencies
- Centralized configuration
- Better maintainability

### ADR-010: Single-File Deployment
**Status**: Accepted
**Decision**: Use single-file self-contained deployment.
**Consequences**:
- Easy deployment (single executable)
- No need to install runtime separately
- Larger file size
- Trimmed assemblies (smaller, but may break reflection)

## Security Architecture

### Authentication & Authorization
```
┌─────────────────────────────────────────────────────────┐
│                 Authentication Flow                        │
│                                                           │
│  User → Login View → UserService → UserRepository        │
│                       ↓                                      │
│              Validate Credentials                         │
│                       ↓                                      │
│              Create Session                                 │
│                       ↓                                      │
│              Return Session Token                         │
│                       ↓                                      │
│              Store in SessionRepository                   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│               Authorization Flow                            │
│                                                           │
│  Request → Check Session Token → Validate Token           │
│                       ↓                                      │
│              Get User from Session                         │
│                       ↓                                      │
│              Get User Permissions                          │
│                       ↓                                      │
│              Check Permission for Action                  │
│                       ↓                                      │
│              Allow/Deny Access                             │
└─────────────────────────────────────────────────────────┘
```

### Data Security
```
┌─────────────────────────────────────────────────────────┐
│                 Evidence Security                           │
│                                                           │
│  • Classification Levels:                                 │
│    - Public: No restrictions                              │
│    - Internal: Organization-only access                   │
│    - Confidential: Limited access                         │
│    - Restricted: Highly sensitive data                    │
│                                                           │
│  • File Integrity:                                        │
│    - SHA256 hash verification                             │
│    - File size validation                                 │
│    - Path validation                                      │
│                                                           │
│  • Storage:                                               │
│    - Encrypted at rest (future)                           │
│    - Access controlled by permissions                     │
│    - Audit logged                                         │
└─────────────────────────────────────────────────────────┘
```

### Audit Logging
```
┌─────────────────────────────────────────────────────────┐
│                 Audit Trail                                 │
│                                                           │
│  All actions are logged with:                             │
│  • Action type (Create, Read, Update, Delete, etc.)       │
│  • Entity type (Threat, Vulnerability, etc.)             │
│  • Entity ID                                              │
│  • User ID                                               │
│  • Timestamp                                             │
│  • IP Address (if available)                             │
│  • User Agent (if available)                             │
│  • Success/Failure status                                │
│  • Error message (if failed)                             │
│  • Old/New values (for updates)                         │
│                                                           │
│  Audit logs are:                                          │
│  • Immutable (cannot be modified)                         │
│  • Retained for configurable period                       │
│  • Searchable and filterable                              │
│  • Exportable                                             │
└─────────────────────────────────────────────────────────┘
```

## Performance Considerations

### Caching Strategy
- **Query Caching**: Cache frequent queries (future implementation)
- **Entity Caching**: Cache reference data (future implementation)
- **UI Caching**: Cache view data for better responsiveness

### Database Optimization
- **Indexing**: Proper indexes on frequently queried columns
- **Query Optimization**: Use efficient LINQ queries
- **Batch Operations**: Batch inserts/updates for performance
- **Lazy Loading**: Lazy load related entities when needed

### UI Performance
- **Virtualization**: Use VirtualizingStackPanel for large lists
- **Data Paging**: Implement paging for large datasets
- **Async Operations**: All I/O operations are async
- **Background Loading**: Load data in background

## Scalability

### Vertical Scaling
- **Database**: SQLite supports large datasets on powerful hardware
- **Application**: Single executable can utilize all available resources

### Horizontal Scaling (Future)
- **Microservices**: Split into separate services (API, UI, Background)
- **Load Balancing**: Distribute load across multiple instances
- **Database Sharding**: Split database across multiple servers

## Extensibility

### Adding New Features
1. **Add Domain Model**: Create new entity in SOCpit.Core
2. **Add Repository Interface**: Create interface in SOCpit.Application.Repositories
3. **Add Repository Implementation**: Create implementation in SOCpit.Storage.Repositories
4. **Add Service**: Create service in SOCpit.Application.Services
5. **Add ViewModel**: Create view model in SOCpit.App.ViewModels
6. **Add View**: Create XAML view in SOCpit.App.Views
7. **Add Navigation**: Update MainViewModel to include new view
8. **Update DbContext**: Add DbSet in SOCpitDbContext
9. **Add Migration**: Create and apply EF Core migration

### Adding New Import/Export
1. **Create Mapper**: Add mapper for new format in SOCpit.Import.Mappers
2. **Create Importer**: Add importer in SOCpit.Import.Importers
3. **Create Exporter**: Add exporter in SOCpit.Import.Exporters
4. **Register Services**: Add to DI container in App.xaml.cs

## Error Handling

### Exception Hierarchy
```
Exception (Base)
├── DomainException
│   ├── ValidationException
│   └── BusinessRuleException
├── InfrastructureException
│   ├── DatabaseException
│   └── RepositoryException
└── ApplicationException
    ├── NotFoundException
    ├── UnauthorizedException
    └── ForbiddenException
```

### Error Handling Strategy
1. **Domain Layer**: Throw domain exceptions for validation errors
2. **Application Layer**: Catch domain exceptions, log, and rethrow as application exceptions
3. **Infrastructure Layer**: Catch database exceptions, log, and rethrow as infrastructure exceptions
4. **Presentation Layer**: Catch all exceptions, log, and display user-friendly messages

### Logging Strategy
- **Debug**: Detailed debugging information
- **Information**: Important application events
- **Warning**: Potential issues or unexpected behavior
- **Error**: Errors that need attention
- **Critical**: Critical failures

## Testing Strategy

### Unit Tests
- Test domain models and validation
- Test repository implementations with in-memory database
- Test services with mocked repositories
- Test view models with mocked services

### Integration Tests
- Test database operations with real database
- Test import/export functionality
- Test end-to-end workflows

### UI Tests
- Test view rendering and data binding
- Test user interactions
- Test navigation flows

### Test Coverage
- **Target**: 80% code coverage
- **Tools**: xUnit, coverlet
- **CI/CD**: Run tests on every push

## Monitoring

### Application Metrics (Future)
- Request count and duration
- Error rates
- Database query performance
- Memory usage
- CPU usage

### Security Metrics (Future)
- Threats detected
- Vulnerabilities found
- Incidents resolved
- Response times
- False positives/negatives

## Documentation

### Code Documentation
- XML documentation for public APIs
- Inline comments for complex logic
- README files for each project

### User Documentation
- User guide
- Installation guide
- Configuration guide
- Troubleshooting guide

### Developer Documentation
- Architecture documentation (this file)
- API documentation
- Contributing guide
- Development setup guide

## Conclusion

SOCpit's architecture is designed for:
- **Maintainability**: Clean separation of concerns, clear responsibilities
- **Extensibility**: Easy to add new features and integrations
- **Testability**: Dependency injection, interfaces, and patterns that support testing
- **Performance**: Async operations, caching, and optimization
- **Security**: Built-in security features and best practices
- **Scalability**: Designed to grow with your needs

This architecture provides a solid foundation for a comprehensive Security Operations Center that can evolve over time while maintaining code quality and developer productivity.
