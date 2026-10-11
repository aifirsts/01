// SOCpit - Security Operations Center
// MATRIX importer for SOCpit project
// Imports data from MATRIX V3 project

using Microsoft.Extensions.Logging;
using SOCpit.Application.Repositories;
using SOCpit.Application.Services;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Import.Importers
{
    /// <summary>Importer for MATRIX V3 data.</summary>
    public sealed class MatrixImporter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MatrixImporter> _logger;

        public MatrixImporter(IUnitOfWork unitOfWork, ILogger<MatrixImporter> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>Import configuration.</summary>
        public sealed class ImportConfig
        {
            public string? SourcePath { get; set; }
            public bool ImportHosts { get; set; } = true;
            public bool ImportNetworks { get; set; } = true;
            public bool ImportThreats { get; set; } = true;
            public bool ImportVulnerabilities { get; set; } = true;
            public bool ImportIncidents { get; set; } = true;
            public bool ImportEvidence { get; set; } = true;
            public string? DefaultSource { get; set; } = "MATRIX V3";
        }

        /// <summary>Import result.</summary>
        public sealed class ImportResult
        {
            public int HostsImported { get; set; }
            public int NetworksImported { get; set; }
            public int ThreatsImported { get; set; }
            public int VulnerabilitiesImported { get; set; }
            public int IncidentsImported { get; set; }
            public int EvidenceImported { get; set; }
            public IReadOnlyList<string> Errors { get; set; } = new List<string>();
            public bool Success => Errors.Count == 0;
        }

        /// <summary>Import all data from MATRIX.</summary>
        public async Task<ImportResult> ImportAllAsync(ImportConfig config, CancellationToken cancellationToken = default)
        {
            var result = new ImportResult();

            try
            {
                if (config.ImportHosts)
                {
                    result.HostsImported = await ImportHostsAsync(config, cancellationToken);
                }

                if (config.ImportNetworks)
                {
                    result.NetworksImported = await ImportNetworksAsync(config, cancellationToken);
                }

                if (config.ImportThreats)
                {
                    result.ThreatsImported = await ImportThreatsAsync(config, cancellationToken);
                }

                if (config.ImportVulnerabilities)
                {
                    result.VulnerabilitiesImported = await ImportVulnerabilitiesAsync(config, cancellationToken);
                }

                if (config.ImportIncidents)
                {
                    result.IncidentsImported = await ImportIncidentsAsync(config, cancellationToken);
                }

                if (config.ImportEvidence)
                {
                    result.EvidenceImported = await ImportEvidenceAsync(config, cancellationToken);
                }

                await _unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                result.Errors = result.Errors.Append(ex.Message).ToList();
                _logger.LogError(ex, "Error importing MATRIX data");
            }

            return result;
        }

        private async Task<int> ImportHostsAsync(ImportConfig config, CancellationToken cancellationToken)
        {
            var count = 0;
            var hostsPath = Path.Combine(config.SourcePath ?? string.Empty, "hosts.json");

            if (!File.Exists(hostsPath))
            {
                _logger.LogWarning("Hosts file not found: {Path}", hostsPath);
                return 0;
            }

            var json = await File.ReadAllTextAsync(hostsPath, cancellationToken);
            var hosts = JsonSerializer.Deserialize<List<HostData>>(json);

            if (hosts == null)
                return 0;

            foreach (var hostData in hosts)
            {
                try
                {
                    var host = Host.Create(
                        id: Guid.NewGuid().ToString(),
                        name: hostData.Name,
                        ipAddress: hostData.IpAddress,
                        macAddress: hostData.MacAddress,
                        type: hostData.Type,
                        status: hostData.Status,
                        operatingSystem: hostData.OperatingSystem,
                        createdAt: DateTimeOffset.UtcNow,
                        tags: hostData.Tags ?? new List<string>(),
                        osVersion: hostData.OsVersion,
                        hostname: hostData.Hostname,
                        domain: hostData.Domain,
                        location: hostData.Location,
                        owner: hostData.Owner);

                    await _unitOfWork.Hosts.AddAsync(host, cancellationToken);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing host: {Name}", hostData.Name);
                }
            }

            _logger.LogInformation("Imported {Count} hosts from MATRIX", count);
            return count;
        }

        private async Task<int> ImportNetworksAsync(ImportConfig config, CancellationToken cancellationToken)
        {
            var count = 0;
            var networksPath = Path.Combine(config.SourcePath ?? string.Empty, "networks.json");

            if (!File.Exists(networksPath))
            {
                _logger.LogWarning("Networks file not found: {Path}", networksPath);
                return 0;
            }

            var json = await File.ReadAllTextAsync(networksPath, cancellationToken);
            var networks = JsonSerializer.Deserialize<List<NetworkData>>(json);

            if (networks == null)
                return 0;

            foreach (var networkData in networks)
            {
                try
                {
                    var network = Network.Create(
                        id: Guid.NewGuid().ToString(),
                        name: networkData.Name,
                        cidr: networkData.Cidr,
                        description: networkData.Description,
                        type: networkData.Type,
                        tags: networkData.Tags ?? new List<string>(),
                        vlanIds: networkData.VlanIds,
                        subnetIds: networkData.SubnetIds);

                    await _unitOfWork.Networks.AddAsync(network, cancellationToken);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing network: {Name}", networkData.Name);
                }
            }

            _logger.LogInformation("Imported {Count} networks from MATRIX", count);
            return count;
        }

        private async Task<int> ImportThreatsAsync(ImportConfig config, CancellationToken cancellationToken)
        {
            var count = 0;
            var threatsPath = Path.Combine(config.SourcePath ?? string.Empty, "threats.json");

            if (!File.Exists(threatsPath))
            {
                _logger.LogWarning("Threats file not found: {Path}", threatsPath);
                return 0;
            }

            var json = await File.ReadAllTextAsync(threatsPath, cancellationToken);
            var threats = JsonSerializer.Deserialize<List<ThreatData>>(json);

            if (threats == null)
                return 0;

            foreach (var threatData in threats)
            {
                try
                {
                    var threat = Threat.Create(
                        id: Guid.NewGuid().ToString(),
                        name: threatData.Name,
                        description: threatData.Description,
                        severity: threatData.Severity,
                        status: threatData.Status,
                        targetId: threatData.TargetId,
                        detectedAt: DateTimeOffset.UtcNow,
                        source: config.DefaultSource ?? "MATRIX V3",
                        tags: threatData.Tags ?? new List<string>(),
                        mitigationSteps: threatData.MitigationSteps,
                        relatedEvidenceIds: threatData.RelatedEvidenceIds);

                    await _unitOfWork.Threats.AddAsync(threat, cancellationToken);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing threat: {Name}", threatData.Name);
                }
            }

            _logger.LogInformation("Imported {Count} threats from MATRIX", count);
            return count;
        }

        private async Task<int> ImportVulnerabilitiesAsync(ImportConfig config, CancellationToken cancellationToken)
        {
            var count = 0;
            var vulnerabilitiesPath = Path.Combine(config.SourcePath ?? string.Empty, "vulnerabilities.json");

            if (!File.Exists(vulnerabilitiesPath))
            {
                _logger.LogWarning("Vulnerabilities file not found: {Path}", vulnerabilitiesPath);
                return 0;
            }

            var json = await File.ReadAllTextAsync(vulnerabilitiesPath, cancellationToken);
            var vulnerabilities = JsonSerializer.Deserialize<List<VulnerabilityData>>(json);

            if (vulnerabilities == null)
                return 0;

            foreach (var vulnerabilityData in vulnerabilities)
            {
                try
                {
                    var vulnerability = Vulnerability.Create(
                        id: Guid.NewGuid().ToString(),
                        cveId: vulnerabilityData.CveId,
                        name: vulnerabilityData.Name,
                        description: vulnerabilityData.Description,
                        severity: vulnerabilityData.Severity,
                        status: vulnerabilityData.Status,
                        targetId: vulnerabilityData.TargetId,
                        publishedAt: DateTimeOffset.UtcNow,
                        fixAvailable: vulnerabilityData.FixAvailable,
                        cvssScore: vulnerabilityData.CvssScore,
                        references: vulnerabilityData.References ?? new List<string>());

                    await _unitOfWork.Vulnerabilities.AddAsync(vulnerability, cancellationToken);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing vulnerability: {Name}", vulnerabilityData.Name);
                }
            }

            _logger.LogInformation("Imported {Count} vulnerabilities from MATRIX", count);
            return count;
        }

        private async Task<int> ImportIncidentsAsync(ImportConfig config, CancellationToken cancellationToken)
        {
            var count = 0;
            var incidentsPath = Path.Combine(config.SourcePath ?? string.Empty, "incidents.json");

            if (!File.Exists(incidentsPath))
            {
                _logger.LogWarning("Incidents file not found: {Path}", incidentsPath);
                return 0;
            }

            var json = await File.ReadAllTextAsync(incidentsPath, cancellationToken);
            var incidents = JsonSerializer.Deserialize<List<IncidentData>>(json);

            if (incidents == null)
                return 0;

            foreach (var incidentData in incidents)
            {
                try
                {
                    var incident = SecurityIncident.Create(
                        id: Guid.NewGuid().ToString(),
                        title: incidentData.Title,
                        description: incidentData.Description,
                        severity: incidentData.Severity,
                        status: incidentData.Status,
                        createdAt: DateTimeOffset.UtcNow,
                        assignedTo: incidentData.AssignedTo,
                        relatedThreatIds: incidentData.RelatedThreatIds ?? new List<string>(),
                        relatedVulnerabilityIds: incidentData.RelatedVulnerabilityIds ?? new List<string>(),
                        impactAnalysis: incidentData.ImpactAnalysis,
                        responseActions: incidentData.ResponseActions);

                    await _unitOfWork.Incidents.AddAsync(incident, cancellationToken);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing incident: {Title}", incidentData.Title);
                }
            }

            _logger.LogInformation("Imported {Count} incidents from MATRIX", count);
            return count;
        }

        private async Task<int> ImportEvidenceAsync(ImportConfig config, CancellationToken cancellationToken)
        {
            var count = 0;
            var evidencePath = Path.Combine(config.SourcePath ?? string.Empty, "evidence.json");

            if (!File.Exists(evidencePath))
            {
                _logger.LogWarning("Evidence file not found: {Path}", evidencePath);
                return 0;
            }

            var json = await File.ReadAllTextAsync(evidencePath, cancellationToken);
            var evidenceList = JsonSerializer.Deserialize<List<EvidenceData>>(json);

            if (evidenceList == null)
                return 0;

            foreach (var evidenceData in evidenceList)
            {
                try
                {
                    var filePath = Path.Combine(config.SourcePath ?? string.Empty, evidenceData.FilePath);
                    if (!File.Exists(filePath))
                    {
                        _logger.LogWarning("Evidence file not found: {Path}", filePath);
                        continue;
                    }

                    var hash = EvidenceService.CalculateHash(filePath);
                    var sizeBytes = EvidenceService.GetFileSize(filePath);

                    var evidence = Evidence.Create(
                        id: Guid.NewGuid().ToString(),
                        name: evidenceData.Name,
                        type: evidenceData.Type,
                        description: evidenceData.Description,
                        status: evidenceData.Status,
                        classification: evidenceData.Classification,
                        source: config.DefaultSource ?? "MATRIX V3",
                        collectedAt: DateTimeOffset.UtcNow,
                        sizeBytes: sizeBytes,
                        hash: hash,
                        filePath: filePath,
                        tags: evidenceData.Tags ?? new List<string>(),
                        relatedIncidentIds: evidenceData.RelatedIncidentIds,
                        relatedThreatIds: evidenceData.RelatedThreatIds,
                        relatedVulnerabilityIds: evidenceData.RelatedVulnerabilityIds,
                        analysisNotes: evidenceData.AnalysisNotes,
                        collectedBy: evidenceData.CollectedBy);

                    await _unitOfWork.Evidence.AddAsync(evidence, cancellationToken);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing evidence: {Name}", evidenceData.Name);
                }
            }

            _logger.LogInformation("Imported {Count} evidence items from MATRIX", count);
            return count;
        }

        // Data transfer objects for JSON deserialization
        private sealed class HostData
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
            public List<string>? Tags { get; set; }
            public string? Owner { get; set; }
        }

        private sealed class NetworkData
        {
            public string Name { get; set; } = string.Empty;
            public string Cidr { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public List<string>? VlanIds { get; set; }
            public List<string>? SubnetIds { get; set; }
            public List<string>? Tags { get; set; }
        }

        private sealed class ThreatData
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public ThreatSeverity Severity { get; set; }
            public ThreatStatus Status { get; set; }
            public string TargetId { get; set; } = string.Empty;
            public List<string>? Tags { get; set; }
            public string? MitigationSteps { get; set; }
            public string? RelatedEvidenceIds { get; set; }
        }

        private sealed class VulnerabilityData
        {
            public string CveId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public VulnerabilitySeverity Severity { get; set; }
            public VulnerabilityStatus Status { get; set; }
            public string TargetId { get; set; } = string.Empty;
            public string? FixAvailable { get; set; }
            public string? CvssScore { get; set; }
            public List<string>? References { get; set; }
        }

        private sealed class IncidentData
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public IncidentSeverity Severity { get; set; }
            public IncidentStatus Status { get; set; }
            public string? AssignedTo { get; set; }
            public List<string>? RelatedThreatIds { get; set; }
            public List<string>? RelatedVulnerabilityIds { get; set; }
            public string? ImpactAnalysis { get; set; }
            public string? ResponseActions { get; set; }
        }

        private sealed class EvidenceData
        {
            public string Name { get; set; } = string.Empty;
            public EvidenceType Type { get; set; }
            public string Description { get; set; } = string.Empty;
            public EvidenceStatus Status { get; set; }
            public EvidenceClassification Classification { get; set; }
            public string FilePath { get; set; } = string.Empty;
            public List<string>? Tags { get; set; }
            public string? RelatedIncidentIds { get; set; }
            public string? RelatedThreatIds { get; set; }
            public string? RelatedVulnerabilityIds { get; set; }
            public string? AnalysisNotes { get; set; }
            public string? CollectedBy { get; set; }
        }
    }
}
