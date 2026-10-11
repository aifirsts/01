// SOCpit - Security Operations Center
// Threat repository implementation
// Based on MATRIX V3 with repository pattern

using Microsoft.EntityFrameworkCore;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Storage.Repositories
{
    /// <summary>Repository implementation for threats.</summary>
    public sealed class ThreatRepository : BaseRepository<Threat>, IThreatRepository
    {
        public ThreatRepository(SOCpitDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Threat>> GetBySeverityAsync(ThreatSeverity severity, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(t => t.Severity == severity)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Threat>> GetByStatusAsync(ThreatStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(t => t.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Threat>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(t => t.TargetId == targetId)
                .ToListAsync(cancellationToken);
        }
    }
}
