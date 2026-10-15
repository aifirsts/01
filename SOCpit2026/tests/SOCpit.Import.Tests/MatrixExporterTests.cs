using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOCpit.Core;
using SOCpit.Import.Exporters;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Import.Tests;

public class MatrixExporterTests
{
    private static MatrixExporter CreateExporter(Storage.UnitOfWork unitOfWork)
        => new(unitOfWork, NullLogger<MatrixExporter>.Instance);

    private static async Task SeedDatabase(Storage.UnitOfWork unitOfWork, string evidencePath)
    {
        await unitOfWork.Hosts.AddAsync(TestData.NewHost(id: "host-1"));
        await unitOfWork.Networks.AddAsync(TestData.NewNetwork(id: "net-1"));
        await unitOfWork.Threats.AddAsync(TestData.NewThreat(id: "threat-1"));
        await unitOfWork.Vulnerabilities.AddAsync(TestData.NewVulnerability(id: "vuln-1"));
        await unitOfWork.Incidents.AddAsync(TestData.NewIncident(id: "incident-1"));
        await unitOfWork.Users.AddAsync(TestData.NewUser(id: "user-1"));
        await unitOfWork.Evidence.AddAsync(TestData.NewEvidence(id: "evidence-1", filePath: evidencePath));
    }

    [Fact]
    public async Task ExportAllAsync_WritesOneJsonFilePerEntityType()
    {
        using var export = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();
        await SeedDatabase(unitOfWork, "/tmp/evidence.log");

        var result = await CreateExporter(unitOfWork).ExportAllAsync(new MatrixExporter.ExportConfig { ExportPath = export.Path });

        Assert.True(result.Success);
        Assert.Equal(1, result.HostsExported);
        Assert.Equal(1, result.NetworksExported);
        Assert.Equal(1, result.ThreatsExported);
        Assert.Equal(1, result.VulnerabilitiesExported);
        Assert.Equal(1, result.IncidentsExported);
        Assert.Equal(1, result.UsersExported);
        Assert.Equal(1, result.EvidenceExported);

        foreach (var fileName in new[] { "hosts.json", "networks.json", "threats.json", "vulnerabilities.json", "incidents.json", "users.json", "evidence.json" })
        {
            Assert.True(File.Exists(export.PathOf(fileName)), $"Expected {fileName} to be written");
        }
    }

    [Fact]
    public async Task ExportAllAsync_WritesWellFormedJsonWithExpectedPayload()
    {
        using var export = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();
        await SeedDatabase(unitOfWork, "/tmp/evidence.log");

        await CreateExporter(unitOfWork).ExportAllAsync(new MatrixExporter.ExportConfig { ExportPath = export.Path });

        using var document = JsonDocument.Parse(File.ReadAllText(export.PathOf("hosts.json")));
        var host = document.RootElement[0];

        Assert.Equal("Edge VPS", host.GetProperty("Name").GetString());
        Assert.Equal("203.0.113.10", host.GetProperty("IpAddress").GetString());

        using var threatDocument = JsonDocument.Parse(File.ReadAllText(export.PathOf("threats.json")));
        Assert.Equal("Suspicious Login", threatDocument.RootElement[0].GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ExportAllAsync_WithoutPath_ReportsError()
    {
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        var result = await CreateExporter(unitOfWork).ExportAllAsync(new MatrixExporter.ExportConfig());

        Assert.False(result.Success);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task ExportAllAsync_HonoursExportFlags()
    {
        using var export = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();
        await SeedDatabase(unitOfWork, "/tmp/evidence.log");

        var result = await CreateExporter(unitOfWork).ExportAllAsync(new MatrixExporter.ExportConfig
        {
            ExportPath = export.Path,
            ExportHosts = false,
            ExportNetworks = false,
            ExportThreats = false,
            ExportVulnerabilities = false,
            ExportIncidents = false,
            ExportEvidence = false,
            ExportUsers = false
        });

        Assert.True(result.Success);
        Assert.Equal(0, result.HostsExported);
        Assert.False(File.Exists(export.PathOf("hosts.json")));
    }

    [Fact]
    public async Task ExportAllAsync_EmptyDatabase_WritesEmptyArrays()
    {
        using var export = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        var result = await CreateExporter(unitOfWork).ExportAllAsync(new MatrixExporter.ExportConfig { ExportPath = export.Path });

        Assert.True(result.Success);
        Assert.Equal(0, result.HostsExported);

        using var document = JsonDocument.Parse(File.ReadAllText(export.PathOf("hosts.json")));
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Equal(0, document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task ExportAllAsync_CreatesMissingDirectory()
    {
        using var export = new TempDirectory();
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        var nested = Path.Combine(export.Path, "nested", "folder");
        var result = await CreateExporter(unitOfWork).ExportAllAsync(new MatrixExporter.ExportConfig { ExportPath = nested });

        Assert.True(result.Success);
        Assert.True(Directory.Exists(nested));
    }
}
