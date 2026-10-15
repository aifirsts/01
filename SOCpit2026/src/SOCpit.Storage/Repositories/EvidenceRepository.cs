// SOCpit - Security Operations Center
// Evidence repository implementation
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
    /// <summary>Repository implementation for evidence.</summary>
    public sealed class EvidenceRepository : BaseRepository<Evidence>, IEvidenceRepository
    {
        public EvidenceRepository(SOCpitDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Evidence>> GetByTypeAsync(EvidenceType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(e => e.Type == type)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Evidence>> GetByStatusAsync(EvidenceStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(e => e.Status == status)
                .ToListAsync(cancellationToken);
        }
    }
}
