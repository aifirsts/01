using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SOCpit.Application.Services;
using SOCpit.Core;
using SOCpit.TestSupport;
using SOCpit.TestSupport.Fakes;
using Xunit;

namespace SOCpit.Application.Tests;

public class ThreatServiceTests
{
    private static (ThreatService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new ThreatService(
            unitOfWork.ThreatsRepo,
            unitOfWork.HostsRepo,
            unitOfWork.EvidenceRepo,
            unitOfWork.AuditLogsRepo,
            NullLogger<ThreatService>.Instance);

        return (service, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_PersistsThreatAndAuditEntry()
    {
        var (service, unitOfWork) = CreateService();

        var threat = await service.CreateAsync(
            name: "SSH brute force",
            description: "Repeated failed logins",
            severity: ThreatSeverity.High,
            status: ThreatStatus.New,
            targetId: "host-1",
            source: "EDR",
            tags: new List<string> { "ssh" },
            createdBy: "analyst-1");

        Assert.Single(unitOfWork.ThreatsRepo.Store);
        Assert.Equal("SSH brute force", threat.Name);
        Assert.Equal("EDR", threat.Source);
        Assert.Equal(ThreatStatus.New, threat.Status);

        var audit = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("Create", audit.Action);
        Assert.Equal("Threat", audit.EntityType);
        Assert.Equal(threat.Id, audit.EntityId);
        Assert.Equal("analyst-1", audit.UserId);
    }

    [Fact]
    public async Task CreateAsync_GeneratesDistinctIds()
    {
        var (service, _) = CreateService();

        var first = await service.CreateAsync("A", "d", ThreatSeverity.Low, ThreatStatus.New, "host-1", "src", new List<string>());
        var second = await service.CreateAsync("B", "d", ThreatSeverity.Low, ThreatStatus.New, "host-1", "src", new List<string>());

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task CreateAsync_DefaultsAuditUserToSystem()
    {
        var (service, unitOfWork) = CreateService();

        await service.CreateAsync("A", "d", ThreatSeverity.Low, ThreatStatus.New, "host-1", "src", new List<string>());

        Assert.Equal("system", unitOfWork.AuditLogsRepo.Entries[0].UserId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsStoredThreat()
    {
        var (service, _) = CreateService();
        var created = await service.CreateAsync("A", "d", ThreatSeverity.Low, ThreatStatus.New, "host-1", "src", new List<string>());

        var loaded = await service.GetByIdAsync(created.Id);

        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var (service, _) = CreateService();
        Assert.Null(await service.GetByIdAsync("does-not-exist"));
    }

    [Fact]
    public async Task QueryMethods_FilterCorrectly()
    {
        var (service, unitOfWork) = CreateService();

        unitOfWork.ThreatsRepo.Seed(
            TestData.NewThreat(id: "threat-1", severity: ThreatSeverity.Critical, status: ThreatStatus.New, targetId: "host-1"),
            TestData.NewThreat(id: "threat-2", severity: ThreatSeverity.Low, status: ThreatStatus.Contained, targetId: "host-1"),
            TestData.NewThreat(id: "threat-3", severity: ThreatSeverity.Low, status: ThreatStatus.New, targetId: "host-2"));

        Assert.Equal(3, await service.CountAsync());
        Assert.Single(await service.GetBySeverityAsync(ThreatSeverity.Critical));
        Assert.Equal(2, (await service.GetBySeverityAsync(ThreatSeverity.Low)).Count);
        Assert.Equal(2, (await service.GetByStatusAsync(ThreatStatus.New)).Count);
        Assert.Equal(2, (await service.GetByTargetIdAsync("host-1")).Count);
        Assert.Equal(3, (await service.GetAllAsync()).Count);
    }

    [Fact]
    public async Task UpdateAsync_AppliesPartialChangesAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var created = await service.CreateAsync("Original", "d", ThreatSeverity.Low, ThreatStatus.New, "host-1", "src", new List<string> { "a" });

        var updated = await service.UpdateAsync(
            created.Id,
            name: "Renamed",
            severity: ThreatSeverity.Critical,
            status: ThreatStatus.Contained,
            updatedBy: "analyst-2");

        Assert.Equal("Renamed", updated.Name);
        Assert.Equal(ThreatSeverity.Critical, updated.Severity);
        Assert.Equal(ThreatStatus.Contained, updated.Status);
        Assert.Equal("d", updated.Description);
        Assert.Equal(new[] { "a" }, updated.Tags);

        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Update" && a.UserId == "analyst-2");
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync("missing", name: "x"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesThreatAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var created = await service.CreateAsync("A", "d", ThreatSeverity.Low, ThreatStatus.New, "host-1", "src", new List<string>());

        Assert.True(await service.DeleteAsync(created.Id, deletedBy: "analyst-1"));
        Assert.Empty(unitOfWork.ThreatsRepo.Store);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Delete" && a.EntityId == created.Id);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalseAndWritesNoAudit()
    {
        var (service, unitOfWork) = CreateService();

        Assert.False(await service.DeleteAsync("missing"));
        Assert.Empty(unitOfWork.AuditLogsRepo.Entries);
    }
}
