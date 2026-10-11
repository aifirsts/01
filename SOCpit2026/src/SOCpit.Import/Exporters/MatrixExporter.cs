// SOCpit - Security Operations Center
// MATRIX exporter for SOCpit project
// Exports data to MATRIX V3 compatible format

using Microsoft.Extensions.Logging;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Import.Exporters
{
    /// <summary>Exporter for MATRIX V3 compatible data.</summary>
    public sealed class MatrixExporter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MatrixExporter> _logger;

        public MatrixExporter(IUnitOfWork unitOfWork, ILogger<MatrixExporter> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>Export configuration.</summary>
        public sealed class ExportConfig
        {
            public string? ExportPath { get; set; }
            public bool ExportHosts { get; set; } = true;
            public bool ExportNetworks { get; set; } = true;
            public bool ExportThreats { get; set; } = true;
            public bool ExportVulnerabilities { get; set; } = true;
            public bool ExportIncidents { get; set; } = true;
            public bool ExportEvidence { get; set; } = true;
            public bool ExportUsers { get; set; } = true;
        }

        /// <summary>Export result.</summary>
        public sealed class ExportResult
        {
            public int HostsExported { get; set; }
            public int NetworksExported { get; set; }
            public int ThreatsExported { get; set; }
            public int VulnerabilitiesExported { get; set; }
            public int IncidentsExported { get; set; }
            public int EvidenceExported { get; set; }
            public int UsersExported { get; set; }
            public IReadOnlyList<string> Errors { get; set; } = new List<string>();
            public bool Success => Errors.Count == 0;
        }

        /// <summary>Export all data to MATRIX format.</summary>
        public async Task<ExportResult> ExportAllAsync(ExportConfig config, CancellationToken cancellationToken = default)
        {
            var result = new ExportResult();

            try
            {
                if (config.ExportPath == null)
                    throw new ArgumentNullException(nameof(config.ExportPath));

                Directory.CreateDirectory(config.ExportPath);

                if (config.ExportHosts)
                {
                    result.HostsExported = await ExportHostsAsync(config, cancellationToken);
                }

                if (config.ExportNetworks)
                {
                    result.NetworksExported = await ExportNetworksAsync(config, cancellationToken);
                }

                if (config.ExportThreats)
                {
                    result.ThreatsExported = await ExportThreatsAsync(config, cancellationToken);
                }

                if (config.ExportVulnerabilities)
                {
                    result.VulnerabilitiesExported = await ExportVulnerabilitiesAsync(config, cancellationToken);
                }

                if (config.ExportIncidents)
                {
                    result.IncidentsExported = await ExportIncidentsAsync(config, cancellationToken);
                }

                if (config.ExportEvidence)
                {
                    result.EvidenceExported = await ExportEvidenceAsync(config, cancellationToken);
                }

                if (config.ExportUsers)
                {
                    result.UsersExported = await ExportUsersAsync(config, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.Errors = result.Errors.Append(ex.Message).ToList();
                _logger.LogError(ex, "Error exporting MATRIX data");
            }

            return result;
        }

        private async Task<int> ExportHostsAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var hosts = await _unitOfWork.Hosts.GetAllAsync(cancellationToken);
            var hostDtos = hosts.Select(h => new HostDto
            {
                Name = h.Name,
                IpAddress = h.IpAddress,
                MacAddress = h.MacAddress,
                Type = h.Type,
                Status = h.Status,
                OperatingSystem = h.OperatingSystem,
                OsVersion = h.OsVersion,
                Hostname = h.Hostname,
                Domain = h.Domain,
                Location = h.Location,
                Tags = h.Tags.ToList(),
                Owner = h.Owner
            }).ToList();

            var json = JsonSerializer.Serialize(hostDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "hosts.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} hosts to {Path}", hostDtos.Count, path);
            return hostDtos.Count;
        }

        private async Task<int> ExportNetworksAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var networks = await _unitOfWork.Networks.GetAllAsync(cancellationToken);
            var networkDtos = networks.Select(n => new NetworkDto
            {
                Name = n.Name,
                Cidr = n.Cidr,
                Description = n.Description,
                Type = n.Type,
                VlanIds = n.VlanIds.ToList(),
                SubnetIds = n.SubnetIds.ToList(),
                Tags = n.Tags.ToList()
            }).ToList();

            var json = JsonSerializer.Serialize(networkDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "networks.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} networks to {Path}", networkDtos.Count, path);
            return networkDtos.Count;
        }

        private async Task<int> ExportThreatsAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var threats = await _unitOfWork.Threats.GetAllAsync(cancellationToken);
            var threatDtos = threats.Select(t => new ThreatDto
            {
                Name = t.Name,
                Description = t.Description,
                Severity = t.Severity,
                Status = t.Status,
                TargetId = t.TargetId,
                DetectedAt = t.DetectedAt,
                Source = t.Source,
                Tags = t.Tags.ToList(),
                MitigationSteps = t.MitigationSteps,
                RelatedEvidenceIds = t.RelatedEvidenceIds
            }).ToList();

            var json = JsonSerializer.Serialize(threatDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "threats.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} threats to {Path}", threatDtos.Count, path);
            return threatDtos.Count;
        }

        private async Task<int> ExportVulnerabilitiesAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var vulnerabilities = await _unitOfWork.Vulnerabilities.GetAllAsync(cancellationToken);
            var vulnerabilityDtos = vulnerabilities.Select(v => new VulnerabilityDto
            {
                CveId = v.CveId,
                Name = v.Name,
                Description = v.Description,
                Severity = v.Severity,
                Status = v.Status,
                TargetId = v.TargetId,
                PublishedAt = v.PublishedAt,
                LastUpdatedAt = v.LastUpdatedAt,
                FixAvailable = v.FixAvailable,
                CvssScore = v.CvssScore,
                References = v.References.ToList()
            }).ToList();

            var json = JsonSerializer.Serialize(vulnerabilityDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "vulnerabilities.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} vulnerabilities to {Path}", vulnerabilityDtos.Count, path);
            return vulnerabilityDtos.Count;
        }

        private async Task<int> ExportIncidentsAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var incidents = await _unitOfWork.Incidents.GetAllAsync(cancellationToken);
            var incidentDtos = incidents.Select(i => new IncidentDto
            {
                Title = i.Title,
                Description = i.Description,
                Severity = i.Severity,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                ResolvedAt = i.ResolvedAt,
                AssignedTo = i.AssignedTo,
                RelatedThreatIds = i.RelatedThreatIds.ToList(),
                RelatedVulnerabilityIds = i.RelatedVulnerabilityIds.ToList(),
                ImpactAnalysis = i.ImpactAnalysis,
                ResponseActions = i.ResponseActions,
                LessonsLearned = i.LessonsLearned
            }).ToList();

            var json = JsonSerializer.Serialize(incidentDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "incidents.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} incidents to {Path}", incidentDtos.Count, path);
            return incidentDtos.Count;
        }

        private async Task<int> ExportEvidenceAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var evidence = await _unitOfWork.Evidence.GetAllAsync(cancellationToken);
            var evidenceDtos = evidence.Select(e => new EvidenceDto
            {
                Name = e.Name,
                Type = e.Type,
                Description = e.Description,
                Status = e.Status,
                Classification = e.Classification,
                Source = e.Source,
                CollectedAt = e.CollectedAt,
                SizeBytes = e.SizeBytes,
                Hash = e.Hash,
                FilePath = e.FilePath,
                Tags = e.Tags.ToList(),
                RelatedIncidentIds = e.RelatedIncidentIds,
                RelatedThreatIds = e.RelatedThreatIds,
                RelatedVulnerabilityIds = e.RelatedVulnerabilityIds,
                AnalysisNotes = e.AnalysisNotes,
                CollectedBy = e.CollectedBy
            }).ToList();

            var json = JsonSerializer.Serialize(evidenceDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "evidence.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} evidence items to {Path}", evidenceDtos.Count, path);
            return evidenceDtos.Count;
        }

        private async Task<int> ExportUsersAsync(ExportConfig config, CancellationToken cancellationToken)
        {
            var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
            var userDtos = users.Select(u => new UserDto
            {
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                Status = u.Status,
                AuthenticationType = u.AuthenticationType,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                Department = u.Department,
                Phone = u.Phone,
                Tags = u.Tags.ToList(),
                PermissionIds = u.PermissionIds.ToList()
            }).ToList();

            var json = JsonSerializer.Serialize(userDtos, new JsonSerializerOptions { WriteIndented = true });
            var path = Path.Combine(config.ExportPath!, "users.json");
            await File.WriteAllTextAsync(path, json, cancellationToken);

            _logger.LogInformation("Exported {Count} users to {Path}", userDtos.Count, path);
            return userDtos.Count;
        }

        // Data transfer objects for JSON serialization
        private sealed class HostDto
        {
            public string Name { get; set; } = string.Empty;
            public string IpAddress { get; set; } = string.Empty;
            public string MacAddress { get; set; } = string.Empty;
            public AssetType Type { get; set; }
            public AssetStatus Status { get; set; }
            public OperatingSystemType OperatingSystem { get; set; }
            public string? OsVersion { get; set; }
            public string? Hostname { get; set; }
            public string? Domain { get; set; }
            public string? Location { get; set; }
            public List<string> Tags { get; set; } = new();
            public string? Owner { get; set; }
        }

        private sealed class NetworkDto
        {
            public string Name { get; set; } = string.Empty;
            public string Cidr { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public List<string> VlanIds { get; set; } = new();
            public List<string> SubnetIds { get; set; } = new();
            public List<string> Tags { get; set; } = new();
        }

        private sealed class ThreatDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public ThreatSeverity Severity { get; set; }
            public ThreatStatus Status { get; set; }
            public string TargetId { get; set; } = string.Empty;
            public DateTimeOffset DetectedAt { get; set; }
            public string Source { get; set; } = string.Empty;
            public List<string> Tags { get; set; } = new();
            public string? MitigationSteps { get; set; }
            public string? RelatedEvidenceIds { get; set; }
        }

        private sealed class VulnerabilityDto
        {
            public string CveId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public VulnerabilitySeverity Severity { get; set; }
            public VulnerabilityStatus Status { get; set; }
            public string TargetId { get; set; } = string.Empty;
            public DateTimeOffset PublishedAt { get; set; }
            public DateTimeOffset? LastUpdatedAt { get; set; }
            public string? FixAvailable { get; set; }
            public string? CvssScore { get; set; }
            public List<string> References { get; set; } = new();
        }

        private sealed class IncidentDto
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public IncidentSeverity Severity { get; set; }
            public IncidentStatus Status { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
            public DateTimeOffset? ResolvedAt { get; set; }
            public string? AssignedTo { get; set; }
            public List<string> RelatedThreatIds { get; set; } = new();
            public List<string> RelatedVulnerabilityIds { get; set; } = new();
            public string? ImpactAnalysis { get; set; }
            public string? ResponseActions { get; set; }
            public string? LessonsLearned { get; set; }
        }

        private sealed class EvidenceDto
        {
            public string Name { get; set; } = string.Empty;
            public EvidenceType Type { get; set; }
            public string Description { get; set; } = string.Empty;
            public EvidenceStatus Status { get; set; }
            public EvidenceClassification Classification { get; set; }
            public string Source { get; set; } = string.Empty;
            public DateTimeOffset CollectedAt { get; set; }
            public long SizeBytes { get; set; }
            public string Hash { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public List<string> Tags { get; set; } = new();
            public string? RelatedIncidentIds { get; set; }
            public string? RelatedThreatIds { get; set; }
            public string? RelatedVulnerabilityIds { get; set; }
            public string? AnalysisNotes { get; set; }
            public string? CollectedBy { get; set; }
        }

        private sealed class UserDto
        {
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public UserRole Role { get; set; }
            public UserStatus Status { get; set; }
            public AuthenticationType AuthenticationType { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? LastLoginAt { get; set; }
            public string? Department { get; set; }
            public string? Phone { get; set; }
            public List<string> Tags { get; set; } = new();
            public List<string> PermissionIds { get; set; } = new();
        }
    }
}
