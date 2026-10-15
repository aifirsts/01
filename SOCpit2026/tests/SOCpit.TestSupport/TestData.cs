// SOCpit - Security Operations Center
// Entity builders and filesystem helpers for tests.

using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SOCpit.TestSupport
{
    /// <summary>Deterministic builders for domain entities used across the test suite.</summary>
    public static class TestData
    {
        private static int _counter;

        /// <summary>A stable "now" so tests never depend on wall-clock timing.</summary>
        public static readonly DateTimeOffset Now = new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

        public static string NewId(string prefix) => $"{prefix}-{Interlocked.Increment(ref _counter)}";

        public static Threat NewThreat(
            string? id = null,
            string name = "Suspicious Login",
            string description = "Repeated failed logins",
            ThreatSeverity severity = ThreatSeverity.High,
            ThreatStatus status = ThreatStatus.New,
            string? targetId = null,
            string source = "EDR",
            IReadOnlyList<string>? tags = null,
            string? mitigationSteps = null,
            string? relatedEvidenceIds = null)
        {
            return Threat.Create(
                id: id ?? NewId("threat"),
                name: name,
                description: description,
                severity: severity,
                status: status,
                targetId: targetId ?? NewId("host"),
                detectedAt: Now,
                source: source,
                tags: tags ?? new List<string> { "test" },
                mitigationSteps: mitigationSteps,
                relatedEvidenceIds: relatedEvidenceIds);
        }

        public static Vulnerability NewVulnerability(
            string? id = null,
            string cveId = "CVE-2026-0001",
            string name = "Outdated TLS stack",
            string description = "Weak cipher suites enabled",
            VulnerabilitySeverity severity = VulnerabilitySeverity.Critical,
            VulnerabilityStatus status = VulnerabilityStatus.Open,
            string? targetId = null,
            string? fixAvailable = "Upgrade to 1.2.3",
            string? cvssScore = "9.1",
            IReadOnlyList<string>? references = null)
        {
            return Vulnerability.Create(
                id: id ?? NewId("vuln"),
                cveId: cveId,
                name: name,
                description: description,
                severity: severity,
                status: status,
                targetId: targetId ?? NewId("host"),
                publishedAt: Now.AddDays(-3),
                fixAvailable: fixAvailable,
                cvssScore: cvssScore,
                lastUpdatedAt: Now,
                references: references ?? new List<string> { "https://nvd.nist.gov/vuln/detail/CVE-2026-0001" });
        }

        public static SecurityIncident NewIncident(
            string? id = null,
            string title = "Perimeter intrusion attempt",
            string description = "Credential stuffing against edge host",
            IncidentSeverity severity = IncidentSeverity.High,
            IncidentStatus status = IncidentStatus.Investigating,
            string? assignedTo = "analyst-1",
            IReadOnlyList<string>? relatedThreatIds = null,
            IReadOnlyList<string>? relatedVulnerabilityIds = null)
        {
            return SecurityIncident.Create(
                id: id ?? NewId("incident"),
                title: title,
                description: description,
                severity: severity,
                status: status,
                createdAt: Now.AddHours(-2),
                assignedTo: assignedTo,
                relatedThreatIds: relatedThreatIds ?? new List<string>(),
                relatedVulnerabilityIds: relatedVulnerabilityIds ?? new List<string>());
        }

        public static Evidence NewEvidence(
            string? id = null,
            string name = "auth.log capture",
            EvidenceType type = EvidenceType.Log,
            string description = "Captured authentication log segment",
            EvidenceStatus status = EvidenceStatus.Analyzed,
            EvidenceClassification classification = EvidenceClassification.Internal,
            string source = "MATRIX V3",
            string? filePath = null,
            long sizeBytes = 1024,
            string? hash = null,
            IReadOnlyList<string>? tags = null)
        {
            return Evidence.Create(
                id: id ?? NewId("evidence"),
                name: name,
                type: type,
                description: description,
                status: status,
                classification: classification,
                source: source,
                collectedAt: Now,
                sizeBytes: sizeBytes,
                hash: hash ?? new string('a', 64),
                filePath: filePath ?? "/tmp/evidence.log",
                tags: tags ?? new List<string> { "test" });
        }

        public static Host NewHost(
            string? id = null,
            string name = "Edge VPS",
            string ipAddress = "203.0.113.10",
            string macAddress = "00:1A:2B:3C:4D:5E",
            AssetType type = AssetType.Server,
            AssetStatus status = AssetStatus.Active,
            OperatingSystemType operatingSystem = OperatingSystemType.Linux,
            IReadOnlyList<string>? tags = null)
        {
            return Host.Create(
                id: id ?? NewId("host"),
                name: name,
                ipAddress: ipAddress,
                macAddress: macAddress,
                type: type,
                status: status,
                operatingSystem: operatingSystem,
                createdAt: Now.AddDays(-30),
                tags: tags ?? new List<string> { "edge" },
                osVersion: "Ubuntu 24.04",
                hostname: "edge-01",
                domain: "neo.infra",
                location: "Cloud EU",
                owner: "NEO",
                lastSeenAt: Now);
        }

        public static Network NewNetwork(
            string? id = null,
            string name = "Internal LAN",
            string cidr = "192.168.1.0/24",
            string description = "Primary internal network",
            string type = "Internal",
            IReadOnlyList<string>? tags = null)
        {
            return Network.Create(
                id: id ?? NewId("net"),
                name: name,
                cidr: cidr,
                description: description,
                type: type,
                tags: tags ?? new List<string> { "internal" },
                vlanIds: new List<string> { "vlan-10" },
                subnetIds: new List<string> { "subnet-10" });
        }

        public static User NewUser(
            string? id = null,
            string username = "analyst",
            string email = "analyst@socpit.local",
            string fullName = "SOC Analyst",
            UserRole role = UserRole.Analyst,
            UserStatus status = UserStatus.Active,
            AuthenticationType authenticationType = AuthenticationType.MultiFactor,
            IReadOnlyList<string>? tags = null,
            IReadOnlyList<string>? permissionIds = null)
        {
            return User.Create(
                id: id ?? NewId("user"),
                username: username,
                email: email,
                fullName: fullName,
                role: role,
                status: status,
                authenticationType: authenticationType,
                createdAt: Now.AddDays(-10),
                tags: tags ?? new List<string> { "soc" },
                permissionIds: permissionIds ?? new List<string> { "perm-read" },
                department: "Security Operations",
                phone: "+65-0000-0000",
                lastLoginAt: Now.AddHours(-1));
        }

        public static AuditLog NewAuditLog(
            string? id = null,
            string action = "Create",
            string entityType = "Threat",
            string entityId = "threat-1",
            string userId = "system")
        {
            return AuditLog.Create(
                id: id ?? NewId("audit"),
                action: action,
                entityType: entityType,
                entityId: entityId,
                userId: userId,
                timestamp: Now,
                details: $"{action} {entityType} {entityId}");
        }

        public static Control NewControl(
            string? id = null,
            string? targetId = null,
            bool required = true,
            ControlApplicability applicability = ControlApplicability.Applicable)
        {
            return Control.Create(
                id: id ?? NewId("ctrl"),
                targetId: targetId ?? NewId("node"),
                criterion: "TLS 1.3 enforced",
                scope: "network",
                method: "automated-scan",
                required: required,
                criticality: Criticality.High,
                ttlSeconds: 3600,
                applicability: applicability,
                rationale: "Mandatory encryption baseline");
        }

        public static ControlEvidence NewControlEvidence(
            string? id = null,
            string? controlId = null,
            EvidenceResult result = EvidenceResult.Pass,
            DateTimeOffset? observedAt = null,
            DateTimeOffset? validUntil = null,
            bool revoked = false,
            IReadOnlyList<string>? supersedes = null)
        {
            return ControlEvidence.Create(
                id: id ?? NewId("cev"),
                controlId: controlId ?? NewId("ctrl"),
                scope: "network",
                source: "scanner",
                method: "automated-scan",
                operatorName: "sec-bot",
                observedAt: observedAt ?? Now.AddMinutes(-10),
                validUntil: validUntil ?? Now.AddMinutes(50),
                result: result,
                provenance: EvidenceProvenance.AutomatedScan,
                revoked: revoked,
                supersedes: supersedes ?? Array.Empty<string>());
        }
    }

    /// <summary>Creates a unique temporary directory that is removed on dispose.</summary>
    public sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "socpit-tests-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string PathOf(string fileName) => System.IO.Path.Combine(Path, fileName);

        public string WriteFile(string fileName, string content)
        {
            var path = PathOf(fileName);
            System.IO.File.WriteAllText(path, content);
            return path;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, recursive: true);
                }
            }
            catch (IOException)
            {
                // Test cleanup is best effort.
            }
        }
    }
}
