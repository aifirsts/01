// SOCpit - Security Operations Center
// Threat repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
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
}
