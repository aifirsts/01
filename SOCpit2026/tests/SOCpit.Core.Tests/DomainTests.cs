using System;
using System.Collections.Generic;
using Xunit;
using SOCpit.Core;
using SOCpit.Core.Models;

namespace SOCpit.Core.Tests;

public class DomainTests
{
    [Fact]
    public void Threat_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = "threat-1";
        var name = "Test Threat";
        var description = "Test Description";
        var severity = SeverityLevel.High;
        var status = ThreatStatus.Active;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;
        var createdBy = "user-1";
        var updatedBy = "user-1";
        var tags = new List<string> { "tag1", "tag2" };
        var affectedAssets = new List<string> { "asset1" };
        var relatedVulnerabilities = new List<string> { "vuln1" };
        var relatedIncidents = new List<string> { "inc1" };
        var mitreTactic = "Initial Access";
        var mitreTechnique = "Phishing";
        var confidenceScore = 0.85;
        var source = "OSINT";
        var externalReference = "ref-1";

        // Act
        var threat = new Threat(
            id, name, description, severity, status, createdAt, updatedAt,
            createdBy, updatedBy, tags, affectedAssets, relatedVulnerabilities,
            relatedIncidents, mitreTactic, mitreTechnique, confidenceScore,
            source, externalReference);

        // Assert
        Assert.Equal(id, threat.Id);
        Assert.Equal(name, threat.Name);
        Assert.Equal(description, threat.Description);
        Assert.Equal(severity, threat.Severity);
        Assert.Equal(status, threat.Status);
        Assert.Equal(createdAt, threat.CreatedAt);
        Assert.Equal(updatedAt, threat.UpdatedAt);
        Assert.Equal(createdBy, threat.CreatedBy);
        Assert.Equal(updatedBy, threat.UpdatedBy);
        Assert.Equal(tags, threat.Tags);
        Assert.Equal(affectedAssets, threat.AffectedAssets);
        Assert.Equal(relatedVulnerabilities, threat.RelatedVulnerabilities);
        Assert.Equal(relatedIncidents, threat.RelatedIncidents);
        Assert.Equal(mitreTactic, threat.MitreTactic);
        Assert.Equal(mitreTechnique, threat.MitreTechnique);
        Assert.Equal(confidenceScore, threat.ConfidenceScore);
        Assert.Equal(source, threat.Source);
        Assert.Equal(externalReference, threat.ExternalReference);
    }

    [Fact]
    public void Vulnerability_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = "vuln-1";
        var cveId = "CVE-2026-1234";
        var title = "Test Vulnerability";
        var description = "Test Description";
        var severity = SeverityLevel.Critical;
        var cvssScore = 9.8;
        var cvssVector = "CVSS:3.1/AV:N/AC:L/PR:N/UI:N/S:U/C:H/I:H/A:H";
        var status = VulnerabilityStatus.Open;
        var discoveredAt = DateTime.UtcNow;
        var patchedAt = (DateTime?)null;
        var affectedSystems = new List<string> { "sys1" };
        var relatedThreats = new List<string> { "threat1" };
        var remediationSteps = "Apply patch";
        var exploitAvailable = "Yes";
        var patchAvailable = "Yes";
        var vendorAdvisory = "https://vendor.com/advisory";

        // Act
        var vuln = new Vulnerability(
            id, cveId, title, description, severity, cvssScore, cvssVector,
            status, discoveredAt, patchedAt, affectedSystems, relatedThreats,
            remediationSteps, exploitAvailable, patchAvailable, vendorAdvisory);

        // Assert
        Assert.Equal(id, vuln.Id);
        Assert.Equal(cveId, vuln.CveId);
        Assert.Equal(title, vuln.Title);
        Assert.Equal(description, vuln.Description);
        Assert.Equal(severity, vuln.Severity);
        Assert.Equal(cvssScore, vuln.CvssScore);
        Assert.Equal(cvssVector, vuln.CvssVector);
        Assert.Equal(status, vuln.Status);
        Assert.Equal(discoveredAt, vuln.DiscoveredAt);
        Assert.Equal(patchedAt, vuln.PatchedAt);
        Assert.Equal(affectedSystems, vuln.AffectedSystems);
        Assert.Equal(relatedThreats, vuln.RelatedThreats);
        Assert.Equal(remediationSteps, vuln.RemediationSteps);
        Assert.Equal(exploitAvailable, vuln.ExploitAvailable);
        Assert.Equal(patchAvailable, vuln.PatchAvailable);
        Assert.Equal(vendorAdvisory, vuln.VendorAdvisory);
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
