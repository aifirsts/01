using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOCpit.Application.Services;
using SOCpit.Core;
using SOCpit.Import.Importers;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Import.Tests;

public class MatrixImporterTests
{
    private static MatrixImporter CreateImporter(Storage.UnitOfWork unitOfWork)
        => new(unitOfWork, NullLogger<MatrixImporter>.Instance);

    private static string HostsJson() => JsonSerializer.Serialize(new[]
    {
        new
        {
            Name = "Edge VPS",
            IpAddress = "203.0.113.10",
            MacAddress = "00:1A:2B:3C:4D:5E",
            Type = AssetType.Server,
            Status = AssetStatus.Active,
            OperatingSystem = OperatingSystemType.Linux,
            OsVersion = "Ubuntu 24.04",
            Hostname = "edge-01",
            Domain = "neo.infra",
            Location = "Cloud EU",
            Tags = new List<string> { "edge" },
            Owner = "NEO"
        }
    });

    private static string NetworksJson() => JsonSerializer.Serialize(new[]
    {
        new
        {
            Name = "Internal LAN",
            Cidr = "192.168.1.0/24",
            Description = "Primary internal network",
            Type = "Internal",
            VlanIds = new List<string> { "vlan-10" },
            SubnetIds = new List<string> { "subnet-10" },
            Tags = new List<string> { "internal" }
        }
    });

    private static string ThreatsJson() => JsonSerializer.Serialize(new[]
    {
        new
        {
            Name = "SSH brute force",
            Description = "Repeated failed logins",
            Severity = ThreatSeverity.High,
            Status = ThreatStatus.Investigating,
            TargetId = "host-vps-01",
            Tags = new List<string> { "ssh" },
            MitigationSteps = "Key-only SSH",
            RelatedEvidenceIds = "evidence-1"
        }
    });

    private static string VulnerabilitiesJson() => JsonSerializer.Serialize(new[]
    {
        new
        {
            CveId = "CVE-2026-1042",
            Name = "OpenSSH race condition",
            Description = "Pre-auth race in sshd",
            Severity = VulnerabilitySeverity.Critical,
            Status = VulnerabilityStatus.Open,
            TargetId = "host-vps-01",
            FixAvailable = "Upgrade to 9.9p2",
            CvssScore = "9.1",
            References = new List<string> { "https://nvd.nist.gov/vuln/detail/CVE-2026-1042" }
        }
    });

    private static string IncidentsJson() => JsonSerializer.Serialize(new[]
    {
        new
        {
            Title = "Perimeter intrusion attempt",
            Description = "Credential stuffing",
            Severity = IncidentSeverity.High,
            Status = IncidentStatus.Investigating,
            AssignedTo = "analyst-1",
            RelatedThreatIds = new List<string> { "threat-1" },
            RelatedVulnerabilityIds = new List<string> { "vuln-1" },
            ImpactAnalysis = "No compromise",
            ResponseActions = "Blocked source IPs"
        }
    });

    private static string EvidenceJson(string fileName) => JsonSerializer.Serialize(new[]
    {
        new
        {
            Name = "VPS auth log",
            Type = EvidenceType.Log,
            Description = "Captured auth.log segment",
            Status = EvidenceStatus.Analyzed,
            Classification = EvidenceClassification.Confidential,
            FilePath = fileName,
            Tags = new List<string> { "ssh" },
            RelatedIncidentIds = "incident-1",
            RelatedThreatIds = "threat-1",
            RelatedVulnerabilityIds = "vuln-1",
            AnalysisNotes = "All attempts rejected",
            CollectedBy = "NEO"
        }
    });

    private static void WriteFullSource(TempDirectory source, string evidenceFileName = "auth.log")
    {
        source.WriteFile("hosts.json", HostsJson());
        source.WriteFile("networks.json", NetworksJson());
        source.WriteFile("threats.json", ThreatsJson());
        source.WriteFile("vulnerabilities.json", VulnerabilitiesJson());
        source.WriteFile("incidents.json", IncidentsJson());
        source.WriteFile("evidence.json", EvidenceJson(evidenceFileName));
    }

    [Fact]
    public async Task ImportAllAsync_WithFullSource_ImportsEveryEntityType()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        WriteFullSource(source);
        var evidenceFile = source.WriteFile("auth.log", "sshd auth log content");

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });

        Assert.True(result.Success);
        Assert.Equal(1, result.HostsImported);
        Assert.Equal(1, result.NetworksImported);
        Assert.Equal(1, result.ThreatsImported);
        Assert.Equal(1, result.VulnerabilitiesImported);
        Assert.Equal(1, result.IncidentsImported);
        Assert.Equal(1, result.EvidenceImported);

        Assert.Equal(1, await unitOfWork.Hosts.CountAsync());
        Assert.Equal(1, await unitOfWork.Networks.CountAsync());
        Assert.Equal(1, await unitOfWork.Threats.CountAsync());
        Assert.Equal(1, await unitOfWork.Vulnerabilities.CountAsync());
        Assert.Equal(1, await unitOfWork.Incidents.CountAsync());
        Assert.Equal(1, await unitOfWork.Evidence.CountAsync());

        var evidence = (await unitOfWork.Evidence.GetAllAsync())[0];
        Assert.Equal(EvidenceService.CalculateHash(evidenceFile), evidence.Hash);
        Assert.Equal(EvidenceService.GetFileSize(evidenceFile), evidence.SizeBytes);
        Assert.Equal(EvidenceClassification.Confidential, evidence.Classification);
    }

    [Fact]
    public async Task ImportAllAsync_MapsHostAndThreatFields()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        source.WriteFile("hosts.json", HostsJson());
        source.WriteFile("threats.json", ThreatsJson());

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });

        Assert.Equal(1, result.HostsImported);
        var host = (await unitOfWork.Hosts.GetAllAsync())[0];
        Assert.Equal("Edge VPS", host.Name);
        Assert.Equal(AssetType.Server, host.Type);
        Assert.Equal(OperatingSystemType.Linux, host.OperatingSystem);
        Assert.Equal("NEO", host.Owner);
        Assert.Equal("Ubuntu 24.04", host.OsVersion);

        var threat = (await unitOfWork.Threats.GetAllAsync())[0];
        Assert.Equal(ThreatSeverity.High, threat.Severity);
        Assert.Equal(ThreatStatus.Investigating, threat.Status);
        Assert.Equal("MATRIX V3", threat.Source);
    }

    [Fact]
    public async Task ImportAllAsync_MissingFiles_ImportNothingButSucceed()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });

        Assert.True(result.Success);
        Assert.Equal(0, result.HostsImported);
        Assert.Equal(0, result.ThreatsImported);
        Assert.Equal(0, await unitOfWork.Threats.CountAsync());
    }

    [Fact]
    public async Task ImportAllAsync_HonoursImportFlags()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        WriteFullSource(source);
        source.WriteFile("auth.log", "content");

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig
        {
            SourcePath = source.Path,
            ImportHosts = false,
            ImportNetworks = false,
            ImportThreats = false,
            ImportVulnerabilities = false,
            ImportIncidents = false,
            ImportEvidence = false
        });

        Assert.Equal(0, result.HostsImported);
        Assert.Equal(0, await unitOfWork.Hosts.CountAsync());
        Assert.Equal(0, await unitOfWork.Networks.CountAsync());
    }

    [Fact]
    public async Task ImportAllAsync_UsesConfiguredDefaultSource()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        source.WriteFile("threats.json", ThreatsJson());

        using var unitOfWork = database.CreateUnitOfWork();
        await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig
        {
            SourcePath = source.Path,
            DefaultSource = "MATRIX-DEV"
        });

        var threat = (await unitOfWork.Threats.GetAllAsync())[0];
        Assert.Equal("MATRIX-DEV", threat.Source);
    }

    [Fact]
    public async Task ImportEvidence_SkipsItemsWhoseFileIsMissing()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        source.WriteFile("evidence.json", EvidenceJson("not-collected.log"));

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });

        Assert.True(result.Success);
        Assert.Equal(0, result.EvidenceImported);
        Assert.Empty(await unitOfWork.Evidence.GetAllAsync());
    }

    [Fact]
    public async Task ImportAllAsync_MalformedJson_ReportsError()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        source.WriteFile("hosts.json", "{ not-valid-json ]");

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ImportAllAsync_SkipsInvalidRowsButImportsValidOnes()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();

        // The second row has an empty name, which the domain model rejects.
        var json = JsonSerializer.Serialize(new object[]
        {
            new
            {
                Name = "Valid host",
                IpAddress = "10.0.0.1",
                MacAddress = "00:00:00:00:00:01",
                Type = AssetType.Server,
                Status = AssetStatus.Active,
                OperatingSystem = OperatingSystemType.Linux,
                OsVersion = (string?)null,
                Hostname = (string?)null,
                Domain = (string?)null,
                Location = (string?)null,
                Tags = new List<string>(),
                Owner = (string?)null
            },
            new
            {
                Name = string.Empty,
                IpAddress = "10.0.0.2",
                MacAddress = "00:00:00:00:00:02",
                Type = AssetType.Server,
                Status = AssetStatus.Active,
                OperatingSystem = OperatingSystemType.Linux,
                OsVersion = (string?)null,
                Hostname = (string?)null,
                Domain = (string?)null,
                Location = (string?)null,
                Tags = new List<string>(),
                Owner = (string?)null
            }
        });
        source.WriteFile("hosts.json", json);

        using var unitOfWork = database.CreateUnitOfWork();
        var result = await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });

        Assert.True(result.Success);
        Assert.Equal(1, result.HostsImported);
        Assert.Equal("Valid host", (await unitOfWork.Hosts.GetAllAsync())[0].Name);
    }

    [Fact]
    public async Task ImportAllAsync_CommittedChanges_AreVisibleInANewContext()
    {
        using var source = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        source.WriteFile("networks.json", NetworksJson());

        using (var unitOfWork = database.CreateUnitOfWork())
        {
            await CreateImporter(unitOfWork).ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = source.Path });
        }

        using var verifyContext = database.CreateContext();
        var stored = verifyContext.Networks.ToList();
        Assert.Single(stored);
        Assert.Equal("192.168.1.0/24", stored[0].Cidr);
    }
}
