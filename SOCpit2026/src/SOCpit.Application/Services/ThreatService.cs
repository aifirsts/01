// SOCpit - Security Operations Center
// Threat service for application layer
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
    /// <summary>Service for managing threats.</summary>
    public sealed class ThreatService
    {
        private readonly IThreatRepository _threatRepository;
        private readonly IHostRepository _hostRepository;
        private readonly IEvidenceRepository _evidenceRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<ThreatService> _logger;

        public ThreatService(
            IThreatRepository threatRepository,
            IHostRepository hostRepository,
            IEvidenceRepository evidenceRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<ThreatService> logger)
        {
            _threatRepository = threatRepository;
            _hostRepository = hostRepository;
            _evidenceRepository = evidenceRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<Threat> CreateAsync(
            string name, string description, ThreatSeverity severity,
            ThreatStatus status, string targetId, string source,
            IReadOnlyList<string> tags, string? mitigationSteps = null,
            string? relatedEvidenceIds = null, string? createdBy = null,
            CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var threat = Threat.Create(
                id: id,
                name: name,
                description: description,
                severity: severity,
                status: status,
                targetId: targetId,
                detectedAt: DateTimeOffset.UtcNow,
                source: source,
                tags: tags,
                mitigationSteps: mitigationSteps,
                relatedEvidenceIds: relatedEvidenceIds);

            await _threatRepository.AddAsync(threat, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Threat",
                entityId: id,
                userId: createdBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Created threat: {name}"), cancellationToken);

            _logger.LogInformation("Created threat {ThreatId}: {Name}", id, name);
            return threat;
        }

        public async Task<Threat?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _threatRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Threat>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _threatRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Threat>> GetBySeverityAsync(ThreatSeverity severity, CancellationToken cancellationToken = default)
        {
            return await _threatRepository.GetBySeverityAsync(severity, cancellationToken);
        }

        public async Task<IReadOnlyList<Threat>> GetByStatusAsync(ThreatStatus status, CancellationToken cancellationToken = default)
        {
            return await _threatRepository.GetByStatusAsync(status, cancellationToken);
        }

        public async Task<IReadOnlyList<Threat>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default)
        {
            return await _threatRepository.GetByTargetIdAsync(targetId, cancellationToken);
        }

        public async Task<Threat> UpdateAsync(
            string id, string? name = null, string? description = null,
            ThreatSeverity? severity = null, ThreatStatus? status = null,
            string? targetId = null, string? source = null,
            IReadOnlyList<string>? tags = null, string? mitigationSteps = null,
            string? relatedEvidenceIds = null, string? updatedBy = null,
            CancellationToken cancellationToken = default)
        {
            var existing = await _threatRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Threat with ID {id} not found");

            var updated = Threat.Create(
                id: id,
                name: name ?? existing.Name,
                description: description ?? existing.Description,
                severity: severity ?? existing.Severity,
                status: status ?? existing.Status,
                targetId: targetId ?? existing.TargetId,
                detectedAt: existing.DetectedAt,
                source: source ?? existing.Source,
                tags: tags ?? existing.Tags,
                mitigationSteps: mitigationSteps ?? existing.MitigationSteps,
                relatedEvidenceIds: relatedEvidenceIds ?? existing.RelatedEvidenceIds);

            await _threatRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Threat",
                entityId: id,
                userId: updatedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Updated threat: {updated.Name}"), cancellationToken);

            _logger.LogInformation("Updated threat {ThreatId}: {Name}", id, updated.Name);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _threatRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return false;

            var result = await _threatRepository.DeleteAsync(id, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Delete",
                entityType: "Threat",
                entityId: id,
                userId: deletedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Deleted threat: {existing.Name}"), cancellationToken);

            _logger.LogInformation("Deleted threat {ThreatId}: {Name}", id, existing.Name);
            return result;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _threatRepository.CountAsync(cancellationToken);
        }
    }
}
