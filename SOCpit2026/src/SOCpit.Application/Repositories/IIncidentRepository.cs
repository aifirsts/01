// SOCpit - Security Operations Center
// Incident repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
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
}
