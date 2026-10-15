// SOCpit - Security Operations Center
// Incident repository implementation
// Based on MATRIX V3 with repository pattern

using Microsoft.EntityFrameworkCore;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Storage.Repositories
{
    /// <summary>Repository implementation for security incidents.</summary>
    public sealed class IncidentRepository : BaseRepository<SecurityIncident>, IIncidentRepository
    {
        public IncidentRepository(SOCpitDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<SecurityIncident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(i => i.Severity == severity)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SecurityIncident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(i => i.Status == status)
                .ToListAsync(cancellationToken);
        }
    }
}
