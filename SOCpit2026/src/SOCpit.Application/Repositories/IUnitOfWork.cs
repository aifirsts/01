// SOCpit - Security Operations Center
// Unit of work interface
// Based on MATRIX V3 with unit of work pattern

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
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
