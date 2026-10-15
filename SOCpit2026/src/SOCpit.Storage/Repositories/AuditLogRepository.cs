// SOCpit - Security Operations Center
// Audit log repository implementation
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
    /// <summary>Repository implementation for audit logs.</summary>
    public sealed class AuditLogRepository : IAuditLogRepository
    {
        private readonly SOCpitDbContext _dbContext;
        private readonly DbSet<AuditLog> _dbSet;

        public AuditLogRepository(SOCpitDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = dbContext.Set<AuditLog>();
        }

        public async Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(auditLog, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return auditLog;
        }

        public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(a => a.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
        {
            // The SQLite provider stores DateTimeOffset as TEXT and cannot translate
            // range comparisons on it, so the audit window is applied in memory.
            // Audit events are ordered and bounded by the caller-supplied window.
            var entries = await _dbSet.AsNoTracking()
                .ToListAsync(cancellationToken);

            return entries
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .OrderBy(a => a.Timestamp)
                .ToList();
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }
    }
}
