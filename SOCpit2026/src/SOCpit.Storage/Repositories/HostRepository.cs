// SOCpit - Security Operations Center
// Host repository implementation
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
    /// <summary>Repository implementation for hosts.</summary>
    public sealed class HostRepository : BaseRepository<Host>, IHostRepository
    {
        public HostRepository(SOCpitDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Host>> GetByTypeAsync(AssetType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(h => h.Type == type)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Host>> GetByStatusAsync(AssetStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(h => h.Status == status)
                .ToListAsync(cancellationToken);
        }
    }
}
