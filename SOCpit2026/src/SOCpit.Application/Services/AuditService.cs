// SOCpit - Security Operations Center
// Audit service for application layer
// Based on MATRIX V3 with service pattern

using Microsoft.Extensions.Logging;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Services
{
    /// <summary>Service for managing audit logs.</summary>
    public sealed class AuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            ILogger<AuditService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<AuditLog> CreateAsync(
            string action, string entityType, string entityId,
            string userId, string? details = null, string? oldValue = null,
            string? newValue = null, string? ipAddress = null,
            string? userAgent = null, bool success = true,
            string? errorMessage = null, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var auditLog = AuditLog.Create(
                id: id,
                action: action,
                entityType: entityType,
                entityId: entityId,
                userId: userId,
                timestamp: DateTimeOffset.UtcNow,
                details: details,
                oldValue: oldValue,
                newValue: newValue,
                ipAddress: ipAddress,
                userAgent: userAgent,
                success: success,
                errorMessage: errorMessage);

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
            _logger.LogInformation("Created audit log {AuditLogId}: {Action} on {EntityType} {EntityId}", id, action, entityType, entityId);
            return auditLog;
        }

        public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByUserAsync(userId, cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.GetByDateRangeAsync(start, end, cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _auditLogRepository.CountAsync(cancellationToken);
        }

        public async Task<AuditLog> LogActionAsync(
            string action, string entityType, string entityId,
            string userId, string? details = null, string? ipAddress = null,
            string? userAgent = null, bool success = true,
            string? errorMessage = null, CancellationToken cancellationToken = default)
        {
            return await CreateAsync(
                action, entityType, entityId, userId, details,
                ipAddress: ipAddress, userAgent: userAgent,
                success: success, errorMessage: errorMessage,
                cancellationToken: cancellationToken);
        }
    }
}
