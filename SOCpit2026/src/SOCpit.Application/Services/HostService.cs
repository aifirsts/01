// SOCpit - Security Operations Center
// Host service for application layer
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
    /// <summary>Service for managing hosts.</summary>
    public sealed class HostService
    {
        private readonly IHostRepository _hostRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<HostService> _logger;

        public HostService(
            IHostRepository hostRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<HostService> logger)
        {
            _hostRepository = hostRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<Host> CreateAsync(
            string name, string ipAddress, string macAddress,
            AssetType type, AssetStatus status, OperatingSystemType operatingSystem,
            IReadOnlyList<string> tags, string? osVersion = null,
            string? hostname = null, string? domain = null,
            string? location = null, string? owner = null,
            string? createdBy = null, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var host = Host.Create(
                id: id,
                name: name,
                ipAddress: ipAddress,
                macAddress: macAddress,
                type: type,
                status: status,
                operatingSystem: operatingSystem,
                createdAt: DateTimeOffset.UtcNow,
                tags: tags,
                osVersion: osVersion,
                hostname: hostname,
                domain: domain,
                location: location,
                owner: owner);

            await _hostRepository.AddAsync(host, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Host",
                entityId: id,
                userId: createdBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Created host: {name} ({ipAddress})"), cancellationToken);

            _logger.LogInformation("Created host {HostId}: {Name} ({IpAddress})", id, name, ipAddress);
            return host;
        }

        public async Task<Host?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _hostRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Host>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _hostRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Host>> GetByTypeAsync(AssetType type, CancellationToken cancellationToken = default)
        {
            return await _hostRepository.GetByTypeAsync(type, cancellationToken);
        }

        public async Task<IReadOnlyList<Host>> GetByStatusAsync(AssetStatus status, CancellationToken cancellationToken = default)
        {
            return await _hostRepository.GetByStatusAsync(status, cancellationToken);
        }

        public async Task<Host> UpdateAsync(
            string id, string? name = null, string? ipAddress = null,
            string? macAddress = null, AssetType? type = null,
            AssetStatus? status = null, OperatingSystemType? operatingSystem = null,
            string? osVersion = null, string? hostname = null,
            string? domain = null, string? location = null,
            string? owner = null, IReadOnlyList<string>? tags = null,
            string? updatedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _hostRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Host with ID {id} not found");

            var updated = Host.Create(
                id: id,
                name: name ?? existing.Name,
                ipAddress: ipAddress ?? existing.IpAddress,
                macAddress: macAddress ?? existing.MacAddress,
                type: type ?? existing.Type,
                status: status ?? existing.Status,
                operatingSystem: operatingSystem ?? existing.OperatingSystem,
                createdAt: existing.CreatedAt,
                tags: tags ?? existing.Tags,
                osVersion: osVersion ?? existing.OsVersion,
                hostname: hostname ?? existing.Hostname,
                domain: domain ?? existing.Domain,
                location: location ?? existing.Location,
                owner: owner ?? existing.Owner,
                lastSeenAt: DateTimeOffset.UtcNow);

            await _hostRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Host",
                entityId: id,
                userId: updatedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Updated host: {updated.Name}"), cancellationToken);

            _logger.LogInformation("Updated host {HostId}: {Name}", id, updated.Name);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _hostRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return false;

            var result = await _hostRepository.DeleteAsync(id, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Delete",
                entityType: "Host",
                entityId: id,
                userId: deletedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Deleted host: {existing.Name}"), cancellationToken);

            _logger.LogInformation("Deleted host {HostId}: {Name}", id, existing.Name);
            return result;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _hostRepository.CountAsync(cancellationToken);
        }
    }
}
