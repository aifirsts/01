// SOCpit - Security Operations Center
// Evidence service for application layer
// Based on MATRIX V3 with service pattern

using Microsoft.Extensions.Logging;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Services
{
    /// <summary>Service for managing evidence.</summary>
    public sealed class EvidenceService
    {
        private readonly IEvidenceRepository _evidenceRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<EvidenceService> _logger;

        public EvidenceService(
            IEvidenceRepository evidenceRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<EvidenceService> logger)
        {
            _evidenceRepository = evidenceRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public static string CalculateHash(string filePath)
        {
            using var sha256 = SHA256.Create();
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
            {
                using var stream = File.OpenRead(filePath);
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            var fallbackBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(filePath ?? string.Empty));
            return BitConverter.ToString(fallbackBytes).Replace("-", "").ToLowerInvariant();
        }

        public static long GetFileSize(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                return new FileInfo(filePath).Length;
            return 0;
        }

        public async Task<Evidence> CreateAsync(
            string name, EvidenceType type, string description,
            EvidenceStatus status, EvidenceClassification classification,
            string source, string filePath, IReadOnlyList<string> tags,
            string? relatedIncidentIds = null, string? relatedThreatIds = null,
            string? relatedVulnerabilityIds = null, string? analysisNotes = null,
            string? collectedBy = null, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var hash = CalculateHash(filePath);
            var sizeBytes = GetFileSize(filePath);

            var evidence = Evidence.Create(
                id: id,
                name: name,
                type: type,
                description: description,
                status: status,
                classification: classification,
                source: source,
                collectedAt: DateTimeOffset.UtcNow,
                sizeBytes: sizeBytes,
                hash: hash,
                filePath: filePath,
                tags: tags,
                relatedIncidentIds: relatedIncidentIds,
                relatedThreatIds: relatedThreatIds,
                relatedVulnerabilityIds: relatedVulnerabilityIds,
                analysisNotes: analysisNotes,
                collectedBy: collectedBy);

            await _evidenceRepository.AddAsync(evidence, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Evidence",
                entityId: id,
                userId: collectedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Created evidence: {name} ({type})"), cancellationToken);

            _logger.LogInformation("Created evidence {EvidenceId}: {Name} ({Type})", id, name, type);
            return evidence;
        }

        public async Task<Evidence?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _evidenceRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Evidence>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _evidenceRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Evidence>> GetByTypeAsync(EvidenceType type, CancellationToken cancellationToken = default)
        {
            return await _evidenceRepository.GetByTypeAsync(type, cancellationToken);
        }

        public async Task<IReadOnlyList<Evidence>> GetByStatusAsync(EvidenceStatus status, CancellationToken cancellationToken = default)
        {
            return await _evidenceRepository.GetByStatusAsync(status, cancellationToken);
        }

        public async Task<Evidence> UpdateAsync(
            string id, string? name = null, EvidenceType? type = null,
            string? description = null, EvidenceStatus? status = null,
            EvidenceClassification? classification = null, string? source = null,
            string? filePath = null, IReadOnlyList<string>? tags = null,
            string? relatedIncidentIds = null, string? relatedThreatIds = null,
            string? relatedVulnerabilityIds = null, string? analysisNotes = null,
            string? updatedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _evidenceRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Evidence with ID {id} not found");

            var updated = Evidence.Create(
                id: id,
                name: name ?? existing.Name,
                type: type ?? existing.Type,
                description: description ?? existing.Description,
                status: status ?? existing.Status,
                classification: classification ?? existing.Classification,
                source: source ?? existing.Source,
                collectedAt: existing.CollectedAt,
                sizeBytes: filePath != null ? GetFileSize(filePath) : existing.SizeBytes,
                hash: filePath != null ? CalculateHash(filePath) : existing.Hash,
                filePath: filePath ?? existing.FilePath,
                tags: tags ?? existing.Tags,
                relatedIncidentIds: relatedIncidentIds ?? existing.RelatedIncidentIds,
                relatedThreatIds: relatedThreatIds ?? existing.RelatedThreatIds,
                relatedVulnerabilityIds: relatedVulnerabilityIds ?? existing.RelatedVulnerabilityIds,
                analysisNotes: analysisNotes ?? existing.AnalysisNotes,
                collectedBy: existing.CollectedBy);

            await _evidenceRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Evidence",
                entityId: id,
                userId: updatedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Updated evidence: {updated.Name}"), cancellationToken);

            _logger.LogInformation("Updated evidence {EvidenceId}: {Name}", id, updated.Name);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _evidenceRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return false;

            var result = await _evidenceRepository.DeleteAsync(id, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Delete",
                entityType: "Evidence",
                entityId: id,
                userId: deletedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Deleted evidence: {existing.Name}"), cancellationToken);

            _logger.LogInformation("Deleted evidence {EvidenceId}: {Name}", id, existing.Name);
            return result;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _evidenceRepository.CountAsync(cancellationToken);
        }
    }
}
