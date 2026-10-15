// SOCpit - Security Operations Center
// Audit log repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
    /// <summary>Repository interface for audit logs.</summary>
    public interface IAuditLogRepository
    {
        Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }
}
