// SOCpit - Security Operations Center
// Threat repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
    /// <summary>Repository interface for threats.</summary>
    public interface IThreatRepository
    {
        Task<Threat> AddAsync(Threat threat, CancellationToken cancellationToken = default);
        Task<Threat?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Threat>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Threat>> GetBySeverityAsync(ThreatSeverity severity, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Threat>> GetByStatusAsync(ThreatStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Threat>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default);
        Task<Threat> UpdateAsync(Threat threat, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for vulnerabilities.</summary>
    public interface IVulnerabilityRepository
    {
        Task<Vulnerability> AddAsync(Vulnerability vulnerability, CancellationToken cancellationToken = default);
        Task<Vulnerability?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Vulnerability>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Vulnerability>> GetBySeverityAsync(VulnerabilitySeverity severity, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Vulnerability>> GetByStatusAsync(VulnerabilityStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Vulnerability>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default);
        Task<Vulnerability> UpdateAsync(Vulnerability vulnerability, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for security incidents.</summary>
    public interface IIncidentRepository
    {
        Task<SecurityIncident> AddAsync(SecurityIncident incident, CancellationToken cancellationToken = default);
        Task<SecurityIncident?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SecurityIncident>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SecurityIncident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SecurityIncident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default);
        Task<SecurityIncident> UpdateAsync(SecurityIncident incident, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for evidence.</summary>
    public interface IEvidenceRepository
    {
        Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default);
        Task<Evidence?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Evidence>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Evidence>> GetByTypeAsync(EvidenceType type, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Evidence>> GetByStatusAsync(EvidenceStatus status, CancellationToken cancellationToken = default);
        Task<Evidence> UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for hosts.</summary>
    public interface IHostRepository
    {
        Task<Host> AddAsync(Host host, CancellationToken cancellationToken = default);
        Task<Host?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Host>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Host>> GetByTypeAsync(AssetType type, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Host>> GetByStatusAsync(AssetStatus status, CancellationToken cancellationToken = default);
        Task<Host> UpdateAsync(Host host, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for networks.</summary>
    public interface INetworkRepository
    {
        Task<Network> AddAsync(Network network, CancellationToken cancellationToken = default);
        Task<Network?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Network>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Network> UpdateAsync(Network network, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for users.</summary>
    public interface IUserRepository
    {
        Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
        Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Repository interface for audit logs.</summary>
    public interface IAuditLogRepository
    {
        Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Unit of work interface for coordinating multiple repositories.</summary>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IThreatRepository Threats { get; }
        IVulnerabilityRepository Vulnerabilities { get; }
        IIncidentRepository Incidents { get; }
        IEvidenceRepository Evidence { get; }
        IHostRepository Hosts { get; }
        INetworkRepository Networks { get; }
        IUserRepository Users { get; }
        IAuditLogRepository AuditLogs { get; }
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}
