using System;
using System.Collections.Generic;
using Xunit;
using SOCpit.Core;

namespace SOCpit.Core.Tests;

public class DomainTests
{
    [Fact]
    public void Threat_Create_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = "threat-1";
        var name = "Test Threat";
        var description = "Test Description";
        var severity = ThreatSeverity.High;
        var status = ThreatStatus.Investigating;
        var targetId = "host-1";
        var detectedAt = DateTimeOffset.UtcNow;
        var source = "OSINT";
        var tags = new List<string> { "tag1", "tag2" };
        var mitigationSteps = "Isolate host";
        var relatedEvidenceIds = "ev-1,ev-2";

        // Act
        var threat = Threat.Create(
            id, name, description, severity, status, targetId, detectedAt,
            source, tags, mitigationSteps, relatedEvidenceIds);

        // Assert
        Assert.Equal(id, threat.Id);
        Assert.Equal(name, threat.Name);
        Assert.Equal(description, threat.Description);
        Assert.Equal(severity, threat.Severity);
        Assert.Equal(status, threat.Status);
        Assert.Equal(targetId, threat.TargetId);
        Assert.Equal(detectedAt, threat.DetectedAt);
        Assert.Equal(source, threat.Source);
        Assert.Equal(tags, threat.Tags);
        Assert.Equal(mitigationSteps, threat.MitigationSteps);
        Assert.Equal(relatedEvidenceIds, threat.RelatedEvidenceIds);
    }

    [Fact]
    public void Vulnerability_Create_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = "vuln-1";
        var cveId = "CVE-2026-1234";
        var name = "Test Vulnerability";
        var description = "Test Description";
        var severity = VulnerabilitySeverity.Critical;
        var status = VulnerabilityStatus.Open;
        var targetId = "host-1";
        var publishedAt = DateTimeOffset.UtcNow;
        var fixAvailable = "Yes - Patch 2026.10";
        var cvssScore = "9.8";
        var lastUpdatedAt = DateTimeOffset.UtcNow;
        var references = new List<string> { "https://nvd.nist.gov/vuln/detail/CVE-2026-1234" };

        // Act
        var vuln = Vulnerability.Create(
            id, cveId, name, description, severity, status,
            targetId, publishedAt, fixAvailable, cvssScore, lastUpdatedAt, references);

        // Assert
        Assert.Equal(id, vuln.Id);
        Assert.Equal(cveId, vuln.CveId);
        Assert.Equal(name, vuln.Name);
        Assert.Equal(description, vuln.Description);
        Assert.Equal(severity, vuln.Severity);
        Assert.Equal(status, vuln.Status);
        Assert.Equal(targetId, vuln.TargetId);
        Assert.Equal(publishedAt, vuln.PublishedAt);
        Assert.Equal(fixAvailable, vuln.FixAvailable);
        Assert.Equal(cvssScore, vuln.CvssScore);
        Assert.Equal(lastUpdatedAt, vuln.LastUpdatedAt);
        Assert.Equal(references, vuln.References);
    }

    [Fact]
    public void SecurityIncident_Create_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = "inc-1";
        var title = "Unauthorized Access Attempt";
        var description = "Multiple failed logins detected on domain controller";
        var severity = IncidentSeverity.High;
        var status = IncidentStatus.Investigating;
        var createdAt = DateTimeOffset.UtcNow;
        var assignedTo = "analyst-1";
        var relatedThreats = new List<string> { "threat-1" };
        var relatedVulns = new List<string> { "vuln-1" };

        // Act
        var incident = SecurityIncident.Create(
            id, title, description, severity, status, createdAt,
            assignedTo, relatedThreats, relatedVulns);

        // Assert
        Assert.Equal(id, incident.Id);
        Assert.Equal(title, incident.Title);
        Assert.Equal(description, incident.Description);
        Assert.Equal(severity, incident.Severity);
        Assert.Equal(status, incident.Status);
        Assert.Equal(createdAt, incident.CreatedAt);
        Assert.Equal(assignedTo, incident.AssignedTo);
        Assert.Equal(relatedThreats, incident.RelatedThreatIds);
        Assert.Equal(relatedVulns, incident.RelatedVulnerabilityIds);
    }

    [Fact]
    public void SecurityPolicy_ComputeEffectiveSecurity_UsesConservativePolicy()
    {
        // Arrange
        var node = new NodeId(Guid.NewGuid());
        var meta = new NodeMeta(
            Title: "Critical Node",
            Summary: null,
            ContentRef: null,
            Security: SecurityLevel.Restricted,
            Origin: DataOrigin.Internal,
            Trust: TrustLevel.Verified,
            CreatedUtc: DateTimeOffset.UtcNow,
            ModifiedUtc: DateTimeOffset.UtcNow,
            Version: 1);

        var evidence = new List<ControlEvidence>
        {
            new(new EvidenceId(Guid.NewGuid()), "High sensitivity log", "ref-1", "HASH1", DateTimeOffset.UtcNow, SecurityLevel.Secret)
        };

        var links = new List<Link>
        {
            new(new LinkId(Guid.NewGuid()), node, new NodeId(Guid.NewGuid()), LinkKind.DependsOn, 1.0, "dep", DateTimeOffset.UtcNow, SecurityLevel.Confidential)
        };

        // Act
        var effective = SecurityPolicy.ComputeEffectiveSecurity(node, meta, evidence, links);

        // Assert
        Assert.Equal(SecurityLevel.Secret, effective);
    }

    [Fact]
    public void GraphValidator_Validate_DetectsInvalidTaskProgress()
    {
        // Arrange
        var nodeId = new NodeId(Guid.NewGuid());
        var nodes = new Dictionary<NodeId, NodeMeta>
        {
            [nodeId] = new NodeMeta("Node 1", null, null, SecurityLevel.Internal, DataOrigin.Internal, TrustLevel.Verified, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 1)
        };
        var tasks = new Dictionary<NodeId, ProjectTask>
        {
            [nodeId] = new ProjectTask(nodeId, ProjectTaskStatus.InProgress, 150, null, null, null)
        };

        // Act
        var issues = GraphValidator.Validate(
            nodes,
            Array.Empty<Link>(),
            tasks,
            new Dictionary<NodeId, IReadOnlyList<ControlEvidence>>());

        // Assert
        Assert.Contains(issues, i => i.Code == "TASK_PROGRESS_RANGE" && i.Severity == ValidationSeverity.Error);
    }
}
