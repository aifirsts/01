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

public class IncidentServiceTests
{
    private static (IncidentService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new IncidentService(
            unitOfWork.IncidentsRepo,
            unitOfWork.ThreatsRepo,
            unitOfWork.VulnerabilitiesRepo,
            unitOfWork.AuditLogsRepo,
            NullLogger<IncidentService>.Instance);

        return (service, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_PersistsIncidentAndAuditEntry()
    {
        var (service, unitOfWork) = CreateService();

        var incident = await service.CreateAsync(
            title: "Perimeter intrusion attempt",
            description: "Credential stuffing detected",
            severity: IncidentSeverity.High,
            status: IncidentStatus.Investigating,
            relatedThreatIds: new List<string> { "threat-1" },
            relatedVulnerabilityIds: new List<string> { "vuln-1" },
            assignedTo: "analyst-1",
            impactAnalysis: "No compromise",
            responseActions: "Blocked source IPs",
            createdBy: "NEO");

        Assert.Single(unitOfWork.IncidentsRepo.Store);
        Assert.Equal("analyst-1", incident.AssignedTo);
        Assert.Equal(new[] { "threat-1" }, incident.RelatedThreatIds);

        var audit = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("SecurityIncident", audit.EntityType);
        Assert.Equal("NEO", audit.UserId);
    }

    [Fact]
    public async Task ResolveAsync_SetsResolvedStatusAndTimestamp()
    {
        var (service, unitOfWork) = CreateService();
        var incident = await service.CreateAsync("Title", "Description", IncidentSeverity.Medium, IncidentStatus.Investigating, new List<string>(), new List<string>());

        var resolved = await service.ResolveAsync(incident.Id, lessonsLearned: "Add rate limiting", resolvedBy: "NEO");

        Assert.Equal(IncidentStatus.Resolved, resolved.Status);
        Assert.NotNull(resolved.ResolvedAt);
        Assert.Equal("Add rate limiting", resolved.LessonsLearned);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Resolve");
    }

    [Fact]
    public async Task ResolveAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ResolveAsync("missing"));
    }

    [Fact]
    public async Task QueryMethods_FilterBySeverityAndStatus()
    {
        var (service, unitOfWork) = CreateService();

        unitOfWork.IncidentsRepo.Seed(
            TestData.NewIncident(id: "inc-1", severity: IncidentSeverity.Critical, status: IncidentStatus.New),
            TestData.NewIncident(id: "inc-2", severity: IncidentSeverity.Low, status: IncidentStatus.Resolved),
            TestData.NewIncident(id: "inc-3", severity: IncidentSeverity.Critical, status: IncidentStatus.Resolved));

        Assert.Equal(3, await service.CountAsync());
        Assert.Equal(2, (await service.GetBySeverityAsync(IncidentSeverity.Critical)).Count);
        Assert.Equal(2, (await service.GetByStatusAsync(IncidentStatus.Resolved)).Count);
        Assert.NotNull(await service.GetByIdAsync("inc-1"));
        Assert.Null(await service.GetByIdAsync("inc-unknown"));
    }

    [Fact]
    public async Task UpdateAsync_AppliesPartialChanges()
    {
        var (service, _) = CreateService();
        var incident = await service.CreateAsync("Title", "Description", IncidentSeverity.Low, IncidentStatus.New, new List<string>(), new List<string>());

        var updated = await service.UpdateAsync(
            incident.Id,
            severity: IncidentSeverity.Critical,
            status: IncidentStatus.Contained,
            responseActions: "Isolated host",
            updatedBy: "analyst");

        Assert.Equal(IncidentSeverity.Critical, updated.Severity);
        Assert.Equal(IncidentStatus.Contained, updated.Status);
        Assert.Equal("Isolated host", updated.ResponseActions);
        Assert.Equal("Title", updated.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesIncidentAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var incident = await service.CreateAsync("Title", "Description", IncidentSeverity.Low, IncidentStatus.New, new List<string>(), new List<string>());

        Assert.True(await service.DeleteAsync(incident.Id, deletedBy: "NEO"));
        Assert.Empty(unitOfWork.IncidentsRepo.Store);
        Assert.StartsWith("Deleted", unitOfWork.AuditLogsRepo.Entries.Last().Details ?? string.Empty);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var (service, _) = CreateService();
        Assert.False(await service.DeleteAsync("missing"));
    }
}
