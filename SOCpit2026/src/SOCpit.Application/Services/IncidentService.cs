// SOCpit - Security Operations Center
// Incident service for application layer
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
    /// <summary>Service for managing security incidents.</summary>
    public sealed class IncidentService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IThreatRepository _threatRepository;
        private readonly IVulnerabilityRepository _vulnerabilityRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<IncidentService> _logger;

        public IncidentService(
            IIncidentRepository incidentRepository,
            IThreatRepository threatRepository,
            IVulnerabilityRepository vulnerabilityRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<IncidentService> logger)
        {
            _incidentRepository = incidentRepository;
            _threatRepository = threatRepository;
            _vulnerabilityRepository = vulnerabilityRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<SecurityIncident> CreateAsync(
            string title, string description, IncidentSeverity severity,
            IncidentStatus status, IReadOnlyList<string> relatedThreatIds,
            IReadOnlyList<string> relatedVulnerabilityIds, string? assignedTo = null,
            string? impactAnalysis = null, string? responseActions = null,
            string? createdBy = null, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var incident = SecurityIncident.Create(
                id: id,
                title: title,
                description: description,
                severity: severity,
                status: status,
                createdAt: DateTimeOffset.UtcNow,
                assignedTo: assignedTo,
                relatedThreatIds: relatedThreatIds,
                relatedVulnerabilityIds: relatedVulnerabilityIds,
                impactAnalysis: impactAnalysis,
                responseActions: responseActions);

            await _incidentRepository.AddAsync(incident, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "SecurityIncident",
                entityId: id,
                userId: createdBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Created incident: {title}"), cancellationToken);

            _logger.LogInformation("Created incident {IncidentId}: {Title}", id, title);
            return incident;
        }

        public async Task<SecurityIncident?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _incidentRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<SecurityIncident>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _incidentRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SecurityIncident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default)
        {
            return await _incidentRepository.GetBySeverityAsync(severity, cancellationToken);
        }

        public async Task<IReadOnlyList<SecurityIncident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default)
        {
            return await _incidentRepository.GetByStatusAsync(status, cancellationToken);
        }

        public async Task<SecurityIncident> UpdateAsync(
            string id, string? title = null, string? description = null,
            IncidentSeverity? severity = null, IncidentStatus? status = null,
            string? assignedTo = null, IReadOnlyList<string>? relatedThreatIds = null,
            IReadOnlyList<string>? relatedVulnerabilityIds = null,
            string? impactAnalysis = null, string? responseActions = null,
            string? lessonsLearned = null, string? updatedBy = null,
            CancellationToken cancellationToken = default)
        {
            var existing = await _incidentRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Incident with ID {id} not found");

            var updated = SecurityIncident.Create(
                id: id,
                title: title ?? existing.Title,
                description: description ?? existing.Description,
                severity: severity ?? existing.Severity,
                status: status ?? existing.Status,
                createdAt: existing.CreatedAt,
                assignedTo: assignedTo ?? existing.AssignedTo,
                relatedThreatIds: relatedThreatIds ?? existing.RelatedThreatIds,
                relatedVulnerabilityIds: relatedVulnerabilityIds ?? existing.RelatedVulnerabilityIds,
                impactAnalysis: impactAnalysis ?? existing.ImpactAnalysis,
                responseActions: responseActions ?? existing.ResponseActions,
                lessonsLearned: lessonsLearned ?? existing.LessonsLearned,
                updatedAt: DateTimeOffset.UtcNow);

            await _incidentRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "SecurityIncident",
                entityId: id,
                userId: updatedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Updated incident: {updated.Title}"), cancellationToken);

            _logger.LogInformation("Updated incident {IncidentId}: {Title}", id, updated.Title);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _incidentRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return false;

            var result = await _incidentRepository.DeleteAsync(id, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Delete",
                entityType: "SecurityIncident",
                entityId: id,
                userId: deletedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Deleted incident: {existing.Title}"), cancellationToken);

            _logger.LogInformation("Deleted incident {IncidentId}: {Title}", id, existing.Title);
            return result;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _incidentRepository.CountAsync(cancellationToken);
        }

        public async Task<SecurityIncident> ResolveAsync(
            string id, string? lessonsLearned = null, string? resolvedBy = null,
            CancellationToken cancellationToken = default)
        {
            var existing = await _incidentRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Incident with ID {id} not found");

            var resolved = SecurityIncident.Create(
                id: id,
                title: existing.Title,
                description: existing.Description,
                severity: existing.Severity,
                status: IncidentStatus.Resolved,
                createdAt: existing.CreatedAt,
                assignedTo: existing.AssignedTo,
                relatedThreatIds: existing.RelatedThreatIds,
                relatedVulnerabilityIds: existing.RelatedVulnerabilityIds,
                impactAnalysis: existing.ImpactAnalysis,
                responseActions: existing.ResponseActions,
                lessonsLearned: lessonsLearned ?? existing.LessonsLearned,
                resolvedAt: DateTimeOffset.UtcNow,
                updatedAt: DateTimeOffset.UtcNow);

            await _incidentRepository.UpdateAsync(resolved, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Resolve",
                entityType: "SecurityIncident",
                entityId: id,
                userId: resolvedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Resolved incident: {resolved.Title}"), cancellationToken);

            _logger.LogInformation("Resolved incident {IncidentId}: {Title}", id, resolved.Title);
            return resolved;
        }
    }
}
