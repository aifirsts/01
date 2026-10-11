// SOCpit - Security Operations Center
// Network service for application layer
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
    /// <summary>Service for managing networks.</summary>
    public sealed class NetworkService
    {
        private readonly INetworkRepository _networkRepository;
        private readonly IHostRepository _hostRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<NetworkService> _logger;

        public NetworkService(
            INetworkRepository networkRepository,
            IHostRepository hostRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<NetworkService> logger)
        {
            _networkRepository = networkRepository;
            _hostRepository = hostRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<Network> CreateAsync(
            string name, string cidr, string description, string type,
            IReadOnlyList<string> tags, IReadOnlyList<string>? vlanIds = null,
            IReadOnlyList<string>? subnetIds = null, string? createdBy = null,
            CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var network = Network.Create(
                id: id,
                name: name,
                cidr: cidr,
                description: description,
                type: type,
                tags: tags,
                vlanIds: vlanIds,
                subnetIds: subnetIds);

            await _networkRepository.AddAsync(network, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "Network",
                entityId: id,
                userId: createdBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Created network: {name} ({cidr})"), cancellationToken);

            _logger.LogInformation("Created network {NetworkId}: {Name} ({Cidr})", id, name, cidr);
            return network;
        }

        public async Task<Network?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _networkRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Network>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _networkRepository.GetAllAsync(cancellationToken);
        }

        public async Task<Network> UpdateAsync(
            string id, string? name = null, string? cidr = null,
            string? description = null, string? type = null,
            IReadOnlyList<string>? vlanIds = null, IReadOnlyList<string>? subnetIds = null,
            IReadOnlyList<string>? tags = null, string? updatedBy = null,
            CancellationToken cancellationToken = default)
        {
            var existing = await _networkRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Network with ID {id} not found");

            var updated = Network.Create(
                id: id,
                name: name ?? existing.Name,
                cidr: cidr ?? existing.Cidr,
                description: description ?? existing.Description,
                type: type ?? existing.Type,
                tags: tags ?? existing.Tags,
                vlanIds: vlanIds ?? existing.VlanIds,
                subnetIds: subnetIds ?? existing.SubnetIds);

            await _networkRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "Network",
                entityId: id,
                userId: updatedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Updated network: {updated.Name}"), cancellationToken);

            _logger.LogInformation("Updated network {NetworkId}: {Name}", id, updated.Name);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _networkRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return false;

            var result = await _networkRepository.DeleteAsync(id, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Delete",
                entityType: "Network",
                entityId: id,
                userId: deletedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Deleted network: {existing.Name}"), cancellationToken);

            _logger.LogInformation("Deleted network {NetworkId}: {Name}", id, existing.Name);
            return result;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _networkRepository.CountAsync(cancellationToken);
        }
    }
}
