// SOCpit - Security Operations Center
// MATRIX mapper for SOCpit project
// Maps between MATRIX V3 and SOCpit data models

using SOCpit.Core;
using System;
using System.Collections.Generic;

namespace SOCpit.Import.Mappers
{
    /// <summary>Mapper for MATRIX V3 data models.</summary>
    public static class MatrixMapper
    {
        /// <summary>Map MATRIX host to SOCpit host.</summary>
        public static Host MapHost(
            string name, string ipAddress, string macAddress,
            string type, string status, string operatingSystem,
            string? osVersion = null, string? hostname = null,
            string? domain = null, string? location = null,
            string? owner = null, IReadOnlyList<string>? tags = null)
        {
            return Host.Create(
                id: Guid.NewGuid().ToString(),
                name: name,
                ipAddress: ipAddress,
                macAddress: macAddress,
                type: ParseAssetType(type),
                status: ParseAssetStatus(status),
                operatingSystem: ParseOperatingSystemType(operatingSystem),
                createdAt: DateTimeOffset.UtcNow,
                tags: tags ?? new List<string>(),
                osVersion: osVersion,
                hostname: hostname,
                domain: domain,
                location: location,
                owner: owner);
        }

        /// <summary>Map MATRIX network to SOCpit network.</summary>
        public static Network MapNetwork(
            string name, string cidr, string description,
            string type, IReadOnlyList<string>? vlanIds = null,
            IReadOnlyList<string>? subnetIds = null, IReadOnlyList<string>? tags = null)
        {
            return Network.Create(
                id: Guid.NewGuid().ToString(),
                name: name,
                cidr: cidr,
                description: description,
                type: type,
                tags: tags ?? new List<string>(),
                vlanIds: vlanIds,
                subnetIds: subnetIds);
        }

        /// <summary>Map MATRIX threat to SOCpit threat.</summary>
        public static Threat MapThreat(
            string name, string description, string severity,
            string status, string targetId, string source,
            IReadOnlyList<string>? tags = null, string? mitigationSteps = null,
            string? relatedEvidenceIds = null)
        {
            return Threat.Create(
                id: Guid.NewGuid().ToString(),
                name: name,
                description: description,
                severity: ParseThreatSeverity(severity),
                status: ParseThreatStatus(status),
                targetId: targetId,
                detectedAt: DateTimeOffset.UtcNow,
                source: source,
                tags: tags ?? new List<string>(),
                mitigationSteps: mitigationSteps,
                relatedEvidenceIds: relatedEvidenceIds);
        }

        /// <summary>Map MATRIX vulnerability to SOCpit vulnerability.</summary>
        public static Vulnerability MapVulnerability(
            string cveId, string name, string description,
            string severity, string status, string targetId,
            string? fixAvailable = null, string? cvssScore = null,
            IReadOnlyList<string>? references = null)
        {
            return Vulnerability.Create(
                id: Guid.NewGuid().ToString(),
                cveId: cveId,
                name: name,
                description: description,
                severity: ParseVulnerabilitySeverity(severity),
                status: ParseVulnerabilityStatus(status),
                targetId: targetId,
                publishedAt: DateTimeOffset.UtcNow,
                fixAvailable: fixAvailable,
                cvssScore: cvssScore,
                references: references ?? new List<string>());
        }

        /// <summary>Map MATRIX incident to SOCpit incident.</summary>
        public static SecurityIncident MapIncident(
            string title, string description, string severity,
            string status, IReadOnlyList<string>? relatedThreatIds = null,
            IReadOnlyList<string>? relatedVulnerabilityIds = null,
            string? assignedTo = null, string? impactAnalysis = null,
            string? responseActions = null)
        {
            return SecurityIncident.Create(
                id: Guid.NewGuid().ToString(),
                title: title,
                description: description,
                severity: ParseIncidentSeverity(severity),
                status: ParseIncidentStatus(status),
                createdAt: DateTimeOffset.UtcNow,
                assignedTo: assignedTo,
                relatedThreatIds: relatedThreatIds ?? new List<string>(),
                relatedVulnerabilityIds: relatedVulnerabilityIds ?? new List<string>(),
                impactAnalysis: impactAnalysis,
                responseActions: responseActions);
        }

        /// <summary>Map MATRIX evidence to SOCpit evidence.</summary>
        public static Evidence MapEvidence(
            string name, string type, string description,
            string status, string classification, string source,
            string filePath, long sizeBytes, string hash,
            IReadOnlyList<string>? tags = null, string? relatedIncidentIds = null,
            string? relatedThreatIds = null, string? relatedVulnerabilityIds = null,
            string? analysisNotes = null, string? collectedBy = null)
        {
            return Evidence.Create(
                id: Guid.NewGuid().ToString(),
                name: name,
                type: ParseEvidenceType(type),
                description: description,
                status: ParseEvidenceStatus(status),
                classification: ParseEvidenceClassification(classification),
                source: source,
                collectedAt: DateTimeOffset.UtcNow,
                sizeBytes: sizeBytes,
                hash: hash,
                filePath: filePath,
                tags: tags ?? new List<string>(),
                relatedIncidentIds: relatedIncidentIds,
                relatedThreatIds: relatedThreatIds,
                relatedVulnerabilityIds: relatedVulnerabilityIds,
                analysisNotes: analysisNotes,
                collectedBy: collectedBy);
        }

        // Parse methods for string to enum conversion
        private static AssetType ParseAssetType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "server" => AssetType.Server,
                "workstation" => AssetType.Workstation,
                "networkdevice" or "network_device" or "network device" => AssetType.NetworkDevice,
                "application" => AssetType.Application,
                "database" => AssetType.Database,
                "cloudresource" or "cloud_resource" or "cloud resource" => AssetType.CloudResource,
                "iotdevice" or "iot_device" or "iot device" => AssetType.IoTDevice,
                "mobiledevice" or "mobile_device" or "mobile device" => AssetType.MobileDevice,
                _ => AssetType.Other
            };
        }

        private static AssetStatus ParseAssetStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "active" => AssetStatus.Active,
                "inactive" => AssetStatus.Inactive,
                "retired" => AssetStatus.Retired,
                "maintenance" => AssetStatus.Maintenance,
                _ => AssetStatus.Unknown
            };
        }

        private static OperatingSystemType ParseOperatingSystemType(string os)
        {
            return os.ToLowerInvariant() switch
            {
                "windows" => OperatingSystemType.Windows,
                "linux" => OperatingSystemType.Linux,
                "macos" or "mac os" or "osx" => OperatingSystemType.macOS,
                "unix" => OperatingSystemType.Unix,
                "ios" => OperatingSystemType.iOS,
                "android" => OperatingSystemType.Android,
                _ => OperatingSystemType.Other
            };
        }

        private static ThreatSeverity ParseThreatSeverity(string severity)
        {
            return severity.ToLowerInvariant() switch
            {
                "low" => ThreatSeverity.Low,
                "medium" => ThreatSeverity.Medium,
                "high" => ThreatSeverity.High,
                "critical" => ThreatSeverity.Critical,
                _ => ThreatSeverity.Low
            };
        }

        private static ThreatStatus ParseThreatStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "new" => ThreatStatus.New,
                "investigating" => ThreatStatus.Investigating,
                "contained" => ThreatStatus.Contained,
                "resolved" => ThreatStatus.Resolved,
                "falsepositive" or "false_positive" or "false positive" => ThreatStatus.FalsePositive,
                _ => ThreatStatus.New
            };
        }

        private static VulnerabilitySeverity ParseVulnerabilitySeverity(string severity)
        {
            return severity.ToLowerInvariant() switch
            {
                "low" => VulnerabilitySeverity.Low,
                "medium" => VulnerabilitySeverity.Medium,
                "high" => VulnerabilitySeverity.High,
                "critical" => VulnerabilitySeverity.Critical,
                _ => VulnerabilitySeverity.Low
            };
        }

        private static VulnerabilityStatus ParseVulnerabilityStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "open" => VulnerabilityStatus.Open,
                "inprogress" or "in_progress" or "in progress" => VulnerabilityStatus.InProgress,
                "fixed" => VulnerabilityStatus.Fixed,
                "notapplicable" or "not_applicable" or "not applicable" => VulnerabilityStatus.NotApplicable,
                "acceptedrisk" or "accepted_risk" or "accepted risk" => VulnerabilityStatus.AcceptedRisk,
                _ => VulnerabilityStatus.Open
            };
        }

        private static IncidentSeverity ParseIncidentSeverity(string severity)
        {
            return severity.ToLowerInvariant() switch
            {
                "low" => IncidentSeverity.Low,
                "medium" => IncidentSeverity.Medium,
                "high" => IncidentSeverity.High,
                "critical" => IncidentSeverity.Critical,
                _ => IncidentSeverity.Low
            };
        }

        private static IncidentStatus ParseIncidentStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "new" => IncidentStatus.New,
                "triage" => IncidentStatus.Triage,
                "investigating" => IncidentStatus.Investigating,
                "contained" => IncidentStatus.Contained,
                "resolved" => IncidentStatus.Resolved,
                "closed" => IncidentStatus.Closed,
                _ => IncidentStatus.New
            };
        }

        private static EvidenceType ParseEvidenceType(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "log" => EvidenceType.Log,
                "screenshot" => EvidenceType.Screenshot,
                "networkcapture" or "network_capture" or "network capture" => EvidenceType.NetworkCapture,
                "file" => EvidenceType.File,
                "memorydump" or "memory_dump" or "memory dump" => EvidenceType.MemoryDump,
                "configuration" => EvidenceType.Configuration,
                "auditlog" or "audit_log" or "audit log" => EvidenceType.AuditLog,
                "alert" => EvidenceType.Alert,
                "report" => EvidenceType.Report,
                _ => EvidenceType.Other
            };
        }

        private static EvidenceStatus ParseEvidenceStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "new" => EvidenceStatus.New,
                "reviewed" => EvidenceStatus.Reviewed,
                "analyzed" => EvidenceStatus.Analyzed,
                "archived" => EvidenceStatus.Archived,
                "deleted" => EvidenceStatus.Deleted,
                _ => EvidenceStatus.New
            };
        }

        private static EvidenceClassification ParseEvidenceClassification(string classification)
        {
            return classification.ToLowerInvariant() switch
            {
                "public" => EvidenceClassification.Public,
                "internal" => EvidenceClassification.Internal,
                "confidential" => EvidenceClassification.Confidential,
                "restricted" => EvidenceClassification.Restricted,
                _ => EvidenceClassification.Public
            };
        }
    }
}
