using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOCpit.Application.Services;
using SOCpit.Core;
using SOCpit.TestSupport;
using SOCpit.TestSupport.Fakes;
using Xunit;

namespace SOCpit.Application.Tests;

public class EvidenceServiceTests
{
    private static (EvidenceService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new EvidenceService(
            unitOfWork.EvidenceRepo,
            unitOfWork.AuditLogsRepo,
            NullLogger<EvidenceService>.Instance);

        return (service, unitOfWork);
    }

    [Fact]
    public void CalculateHash_ExistingFile_ReturnsSha256OfContent()
    {
        using var temp = new TempDirectory();
        var content = "2026-10-15 sshd[1]: Failed password for root";
        var path = temp.WriteFile("auth.log", content);

        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant();

        Assert.Equal(expected, EvidenceService.CalculateHash(path));
    }

    [Fact]
    public void CalculateHash_MissingFile_IsDeterministicAndPathDependent()
    {
        var first = EvidenceService.CalculateHash("/missing/one.log");
        var second = EvidenceService.CalculateHash("/missing/one.log");
        var other = EvidenceService.CalculateHash("/missing/two.log");

        Assert.Equal(first, second);
        Assert.NotEqual(first, other);
        Assert.Equal(64, first.Length);
    }

    [Fact]
    public void CalculateHash_EmptyInput_ReturnsHashOfEmptyString()
    {
        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Empty))).ToLowerInvariant();
        Assert.Equal(expected, EvidenceService.CalculateHash(string.Empty));
    }

    [Fact]
    public void GetFileSize_ExistingFile_ReturnsByteCount()
    {
        using var temp = new TempDirectory();
        var path = temp.WriteFile("capture.txt", "12345");

        Assert.Equal(5, EvidenceService.GetFileSize(path));
    }

    [Fact]
    public void GetFileSize_MissingFile_ReturnsZero()
    {
        Assert.Equal(0, EvidenceService.GetFileSize("/missing/file.bin"));
    }

    [Fact]
    public async Task CreateAsync_WithExistingFile_ComputesHashAndSize()
    {
        var (service, unitOfWork) = CreateService();
        using var temp = new TempDirectory();
        var path = temp.WriteFile("events.evtx", "log-content");

        var evidence = await service.CreateAsync(
            name: "Workstation events",
            type: EvidenceType.AuditLog,
            description: "Windows event export",
            status: EvidenceStatus.New,
            classification: EvidenceClassification.Internal,
            source: "EDR",
            filePath: path,
            tags: new List<string> { "windows" },
            relatedIncidentIds: "incident-1",
            collectedBy: "NEO");

        Assert.Equal(11, evidence.SizeBytes);
        Assert.Equal(EvidenceService.CalculateHash(path), evidence.Hash);
        Assert.Equal("NEO", evidence.CollectedBy);
        Assert.Single(unitOfWork.EvidenceRepo.Store);

        var audit = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("Evidence", audit.EntityType);
        Assert.Contains("Created evidence", audit.Details);
    }

    [Fact]
    public async Task CreateAsync_WithMissingFile_RecordsZeroSizeAndPathHash()
    {
        var (service, _) = CreateService();

        var evidence = await service.CreateAsync(
            "Missing capture", EvidenceType.Log, "Not collected yet",
            EvidenceStatus.New, EvidenceClassification.Public,
            "manual", "/missing/never-collected.log", new List<string>());

        Assert.Equal(0, evidence.SizeBytes);
        Assert.Equal(EvidenceService.CalculateHash("/missing/never-collected.log"), evidence.Hash);
    }

    [Fact]
    public async Task QueryMethods_FilterByTypeAndStatus()
    {
        var (service, unitOfWork) = CreateService();

        unitOfWork.EvidenceRepo.Seed(
            TestData.NewEvidence(id: "evidence-1", type: EvidenceType.Log, status: EvidenceStatus.New),
            TestData.NewEvidence(id: "evidence-2", type: EvidenceType.Screenshot, status: EvidenceStatus.Reviewed),
            TestData.NewEvidence(id: "evidence-3", type: EvidenceType.Log, status: EvidenceStatus.Reviewed));

        Assert.Equal(3, await service.CountAsync());
        Assert.Equal(2, (await service.GetByTypeAsync(EvidenceType.Log)).Count);
        Assert.Equal(2, (await service.GetByStatusAsync(EvidenceStatus.Reviewed)).Count);
        Assert.Equal(3, (await service.GetAllAsync()).Count);
        Assert.NotNull(await service.GetByIdAsync("evidence-1"));
        Assert.Null(await service.GetByIdAsync("evidence-missing"));
    }

    [Fact]
    public async Task UpdateAsync_WithNewFilePath_RecomputesHashAndSize()
    {
        var (service, _) = CreateService();
        using var temp = new TempDirectory();
        var originalPath = temp.WriteFile("original.log", "abc");
        var replacementPath = temp.WriteFile("replacement.log", "abcdef");

        var created = await service.CreateAsync(
            "Capture", EvidenceType.Log, "desc", EvidenceStatus.New, EvidenceClassification.Internal,
            "manual", originalPath, new List<string>());

        var updated = await service.UpdateAsync(
            created.Id,
            status: EvidenceStatus.Analyzed,
            filePath: replacementPath,
            analysisNotes: "Rotated capture",
            updatedBy: "analyst");

        Assert.Equal(EvidenceStatus.Analyzed, updated.Status);
        Assert.Equal(6, updated.SizeBytes);
        Assert.Equal(EvidenceService.CalculateHash(replacementPath), updated.Hash);
        Assert.Equal("Rotated capture", updated.AnalysisNotes);
        Assert.Equal("Capture", updated.Name);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync("missing", status: EvidenceStatus.Archived));
    }

    [Fact]
    public async Task DeleteAsync_RemovesEvidenceAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var created = await service.CreateAsync(
            "Capture", EvidenceType.Log, "desc", EvidenceStatus.New, EvidenceClassification.Internal,
            "manual", "/tmp/capture.log", new List<string>(), collectedBy: "NEO");

        Assert.True(await service.DeleteAsync(created.Id, deletedBy: "NEO"));
        Assert.Empty(unitOfWork.EvidenceRepo.Store);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Delete" && a.EntityId == created.Id);
        Assert.False(await service.DeleteAsync("missing"));
    }
}
