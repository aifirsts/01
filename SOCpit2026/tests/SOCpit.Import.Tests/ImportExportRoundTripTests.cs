using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOCpit.Application.Services;
using SOCpit.Core;
using SOCpit.Import.Exporters;
using SOCpit.Import.Importers;
using SOCpit.Storage.Repositories;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Import.Tests;

/// <summary>
/// End-to-end contract: everything SOCpit exports for MATRIX V3 can be imported
/// back into a fresh SOCpit database without losing records or key fields.
/// </summary>
public class ImportExportRoundTripTests
{
    [Fact]
    public async Task ExportThenImport_PreservesRowCountsAndKeyFields()
    {
        using var exportDirectory = new TempDirectory();
        using var sourceDatabase = SqliteTestDatabase.Create();
        using var targetDatabase = SqliteTestDatabase.Create();

        // Seed the source database with a representative data set.
        using (var sourceUnitOfWork = sourceDatabase.CreateUnitOfWork())
        {
            await sourceUnitOfWork.Hosts.AddAsync(TestData.NewHost(id: "host-1", name: "Edge VPS"));
            await sourceUnitOfWork.Hosts.AddAsync(TestData.NewHost(id: "host-2", name: "Workstation"));
            await sourceUnitOfWork.Networks.AddAsync(TestData.NewNetwork(id: "net-1"));
            await sourceUnitOfWork.Threats.AddAsync(TestData.NewThreat(id: "threat-1", name: "SSH brute force"));
            await sourceUnitOfWork.Vulnerabilities.AddAsync(TestData.NewVulnerability(id: "vuln-1", cveId: "CVE-2026-1042"));
            await sourceUnitOfWork.Incidents.AddAsync(TestData.NewIncident(id: "incident-1", title: "Perimeter intrusion"));
            await sourceUnitOfWork.Users.AddAsync(TestData.NewUser(id: "user-1", username: "neo"));

            var exporter = new MatrixExporter(sourceUnitOfWork, NullLogger<MatrixExporter>.Instance);
            var exportResult = await exporter.ExportAllAsync(new MatrixExporter.ExportConfig { ExportPath = exportDirectory.Path });
            Assert.True(exportResult.Success);
        }

        // Import the exported snapshot into a brand new database.
        using (var targetUnitOfWork = targetDatabase.CreateUnitOfWork())
        {
            var importer = new MatrixImporter(targetUnitOfWork, NullLogger<MatrixImporter>.Instance);
            var importResult = await importer.ImportAllAsync(new MatrixImporter.ImportConfig { SourcePath = exportDirectory.Path });

            Assert.True(importResult.Success);
            Assert.Equal(2, importResult.HostsImported);
            Assert.Equal(1, importResult.NetworksImported);
            Assert.Equal(1, importResult.ThreatsImported);
            Assert.Equal(1, importResult.VulnerabilitiesImported);
            Assert.Equal(1, importResult.IncidentsImported);
        }

        using var verifyContext = targetDatabase.CreateContext();

        var hosts = await new HostRepository(verifyContext).GetAllAsync();
        Assert.Equal(2, hosts.Count);

        var threats = await new ThreatRepository(verifyContext).GetAllAsync();
        Assert.Equal("SSH brute force", threats[0].Name);
        Assert.Equal(ThreatSeverity.High, threats[0].Severity);

        var vulnerabilities = await new VulnerabilityRepository(verifyContext).GetAllAsync();
        Assert.Equal("CVE-2026-1042", vulnerabilities[0].CveId);

        var incidents = await new IncidentRepository(verifyContext).GetAllAsync();
        Assert.Equal("Perimeter intrusion", incidents[0].Title);
        Assert.Equal(IncidentStatus.Investigating, incidents[0].Status);

        var networks = await new NetworkRepository(verifyContext).GetAllAsync();
        Assert.Equal(new[] { "vlan-10" }, networks[0].VlanIds);
    }

    [Fact]
    public async Task ExportThenImport_EvidenceWithExistingFile_KeepsHashAndSize()
    {
        using var sourceFiles = new TempDirectory();
        using var exportDirectory = new TempDirectory();
        using var sourceDatabase = SqliteTestDatabase.Create();
        using var targetDatabase = SqliteTestDatabase.Create();

        const string evidenceContent = "captured authentication log";
        var evidenceFile = sourceFiles.WriteFile("auth.log", evidenceContent);

        using (var sourceUnitOfWork = sourceDatabase.CreateUnitOfWork())
        {
            await sourceUnitOfWork.Evidence.AddAsync(TestData.NewEvidence(id: "evidence-1", filePath: evidenceFile));

            var exporter = new MatrixExporter(sourceUnitOfWork, NullLogger<MatrixExporter>.Instance);
            var exportResult = await exporter.ExportAllAsync(new MatrixExporter.ExportConfig
            {
                ExportPath = exportDirectory.Path,
                ExportHosts = false
            });
            Assert.True(exportResult.Success);
        }

        using (var targetUnitOfWork = targetDatabase.CreateUnitOfWork())
        {
            var importer = new MatrixImporter(targetUnitOfWork, NullLogger<MatrixImporter>.Instance);
            var importResult = await importer.ImportAllAsync(new MatrixImporter.ImportConfig
            {
                SourcePath = exportDirectory.Path,
                ImportHosts = false,
                ImportNetworks = false,
                ImportThreats = false,
                ImportVulnerabilities = false,
                ImportIncidents = false
            });

            Assert.Equal(1, importResult.EvidenceImported);
        }

        using var verifyContext = targetDatabase.CreateContext();
        var evidence = await new EvidenceRepository(verifyContext).GetAllAsync();

        Assert.Single(evidence);
        Assert.Equal(EvidenceService.CalculateHash(evidenceFile), evidence[0].Hash);
        Assert.Equal(new FileInfo(evidenceFile).Length, evidence[0].SizeBytes);
    }
}
