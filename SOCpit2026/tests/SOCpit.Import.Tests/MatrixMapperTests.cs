using System.Collections.Generic;
using SOCpit.Core;
using SOCpit.Import.Mappers;
using Xunit;

namespace SOCpit.Import.Tests;

/// <summary>
/// MATRIX V3 string values must map onto the SOCpit domain enums, with
/// tolerant aliases and safe fallbacks for unknown input.
/// </summary>
public class MatrixMapperTests
{
    [Theory]
    [InlineData("server", AssetType.Server)]
    [InlineData("Server", AssetType.Server)]
    [InlineData("workstation", AssetType.Workstation)]
    [InlineData("network device", AssetType.NetworkDevice)]
    [InlineData("network_device", AssetType.NetworkDevice)]
    [InlineData("networkdevice", AssetType.NetworkDevice)]
    [InlineData("cloud resource", AssetType.CloudResource)]
    [InlineData("iot device", AssetType.IoTDevice)]
    [InlineData("mobile device", AssetType.MobileDevice)]
    [InlineData("database", AssetType.Database)]
    [InlineData("unknown-thing", AssetType.Other)]
    public void MapHost_MapsAssetTypes(string matrixType, AssetType expected)
    {
        var host = MatrixMapper.MapHost("host", "10.0.0.1", "00:00:00:00:00:01", matrixType, "active", "linux");
        Assert.Equal(expected, host.Type);
    }

    [Theory]
    [InlineData("active", AssetStatus.Active)]
    [InlineData("inactive", AssetStatus.Inactive)]
    [InlineData("retired", AssetStatus.Retired)]
    [InlineData("maintenance", AssetStatus.Maintenance)]
    [InlineData("nonsense", AssetStatus.Unknown)]
    public void MapHost_MapsAssetStatuses(string matrixStatus, AssetStatus expected)
    {
        var host = MatrixMapper.MapHost("host", "10.0.0.1", "00:00:00:00:00:01", "server", matrixStatus, "linux");
        Assert.Equal(expected, host.Status);
    }

    [Theory]
    [InlineData("windows", OperatingSystemType.Windows)]
    [InlineData("linux", OperatingSystemType.Linux)]
    [InlineData("macos", OperatingSystemType.macOS)]
    [InlineData("mac os", OperatingSystemType.macOS)]
    [InlineData("osx", OperatingSystemType.macOS)]
    [InlineData("unix", OperatingSystemType.Unix)]
    [InlineData("ios", OperatingSystemType.iOS)]
    [InlineData("android", OperatingSystemType.Android)]
    [InlineData("plan9", OperatingSystemType.Other)]
    public void MapHost_MapsOperatingSystems(string matrixOs, OperatingSystemType expected)
    {
        var host = MatrixMapper.MapHost("host", "10.0.0.1", "00:00:00:00:00:01", "server", "active", matrixOs);
        Assert.Equal(expected, host.OperatingSystem);
    }

    [Fact]
    public void MapHost_ProducesValidDomainEntityWithGeneratedId()
    {
        var host = MatrixMapper.MapHost(
            "Edge VPS", "203.0.113.10", "00:1A:2B:3C:4D:5E", "server", "active", "linux",
            osVersion: "Ubuntu 24.04", hostname: "edge-01", domain: "neo.infra",
            location: "Cloud EU", owner: "NEO", tags: new List<string> { "edge" });

        Assert.False(string.IsNullOrWhiteSpace(host.Id));
        Assert.Equal("Edge VPS", host.Name);
        Assert.Equal("203.0.113.10", host.IpAddress);
        Assert.Equal("NEO", host.Owner);
        Assert.Single(host.Tags);
    }

    [Fact]
    public void MapHost_DefaultsTagsToEmptyList()
    {
        var host = MatrixMapper.MapHost("host", "10.0.0.1", "00:00:00:00:00:01", "server", "active", "linux");
        Assert.Empty(host.Tags);
    }

    [Fact]
    public void MapNetwork_MapsFieldsAndCollections()
    {
        var network = MatrixMapper.MapNetwork(
            "Internal LAN", "192.168.1.0/24", "Primary network", "Internal",
            vlanIds: new List<string> { "vlan-10" }, subnetIds: new List<string> { "subnet-10" },
            tags: new List<string> { "internal" });

        Assert.Equal("192.168.1.0/24", network.Cidr);
        Assert.Equal("Internal", network.Type);
        Assert.Equal(new[] { "vlan-10" }, network.VlanIds);
        Assert.Equal(new[] { "subnet-10" }, network.SubnetIds);
    }

    [Theory]
    [InlineData("low", ThreatSeverity.Low)]
    [InlineData("high", ThreatSeverity.High)]
    [InlineData("CRITICAL", ThreatSeverity.Critical)]
    [InlineData("weird", ThreatSeverity.Low)]
    public void MapThreat_MapsSeverity(string severity, ThreatSeverity expected)
    {
        var threat = MatrixMapper.MapThreat("name", "desc", severity, "new", "host-1", "MATRIX V3");
        Assert.Equal(expected, threat.Severity);
    }

    [Theory]
    [InlineData("new", ThreatStatus.New)]
    [InlineData("investigating", ThreatStatus.Investigating)]
    [InlineData("contained", ThreatStatus.Contained)]
    [InlineData("resolved", ThreatStatus.Resolved)]
    [InlineData("false positive", ThreatStatus.FalsePositive)]
    [InlineData("false_positive", ThreatStatus.FalsePositive)]
    [InlineData("unknown", ThreatStatus.New)]
    public void MapThreat_MapsStatus(string status, ThreatStatus expected)
    {
        var threat = MatrixMapper.MapThreat("name", "desc", "high", status, "host-1", "MATRIX V3");
        Assert.Equal(expected, threat.Status);
    }

    [Theory]
    [InlineData("in progress", VulnerabilityStatus.InProgress)]
    [InlineData("in_progress", VulnerabilityStatus.InProgress)]
    [InlineData("not applicable", VulnerabilityStatus.NotApplicable)]
    [InlineData("accepted risk", VulnerabilityStatus.AcceptedRisk)]
    [InlineData("fixed", VulnerabilityStatus.Fixed)]
    [InlineData("banana", VulnerabilityStatus.Open)]
    public void MapVulnerability_MapsStatus(string status, VulnerabilityStatus expected)
    {
        var vulnerability = MatrixMapper.MapVulnerability("CVE-2026-1", "name", "desc", "high", status, "host-1");
        Assert.Equal(expected, vulnerability.Status);
    }

    [Theory]
    [InlineData("triage", IncidentStatus.Triage)]
    [InlineData("closed", IncidentStatus.Closed)]
    [InlineData("resolved", IncidentStatus.Resolved)]
    [InlineData("banana", IncidentStatus.New)]
    public void MapIncident_MapsStatus(string status, IncidentStatus expected)
    {
        var incident = MatrixMapper.MapIncident("title", "desc", "critical", status);
        Assert.Equal(expected, incident.Status);
        Assert.Equal(IncidentSeverity.Critical, incident.Severity);
    }

    [Theory]
    [InlineData("log", EvidenceType.Log)]
    [InlineData("network capture", EvidenceType.NetworkCapture)]
    [InlineData("memory dump", EvidenceType.MemoryDump)]
    [InlineData("audit log", EvidenceType.AuditLog)]
    [InlineData("audit_log", EvidenceType.AuditLog)]
    [InlineData("unknown", EvidenceType.Other)]
    public void MapEvidence_MapsType(string type, EvidenceType expected)
    {
        var evidence = MatrixMapper.MapEvidence("name", type, "desc", "new", "internal", "MATRIX V3", "/tmp/file.log", 10, "hash");
        Assert.Equal(expected, evidence.Type);
    }

    [Theory]
    [InlineData("reviewed", EvidenceStatus.Reviewed)]
    [InlineData("analyzed", EvidenceStatus.Analyzed)]
    [InlineData("archived", EvidenceStatus.Archived)]
    [InlineData("deleted", EvidenceStatus.Deleted)]
    [InlineData("weird", EvidenceStatus.New)]
    public void MapEvidence_MapsStatus(string status, EvidenceStatus expected)
    {
        var evidence = MatrixMapper.MapEvidence("name", "log", "desc", status, "internal", "MATRIX V3", "/tmp/file.log", 10, "hash");
        Assert.Equal(expected, evidence.Status);
    }

    [Theory]
    [InlineData("public", EvidenceClassification.Public)]
    [InlineData("internal", EvidenceClassification.Internal)]
    [InlineData("confidential", EvidenceClassification.Confidential)]
    [InlineData("restricted", EvidenceClassification.Restricted)]
    [InlineData("weird", EvidenceClassification.Public)]
    public void MapEvidence_MapsClassification(string classification, EvidenceClassification expected)
    {
        var evidence = MatrixMapper.MapEvidence("name", "log", "desc", "new", classification, "MATRIX V3", "/tmp/file.log", 10, "hash");
        Assert.Equal(expected, evidence.Classification);
    }
}
