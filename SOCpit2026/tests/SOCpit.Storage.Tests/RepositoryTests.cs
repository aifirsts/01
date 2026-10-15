using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOCpit.Core;
using SOCpit.Storage.Repositories;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Storage.Tests;

/// <summary>
/// Repository layer verified against a real SQLite database schema
/// (created from the production <see cref="SOCpitDbContext"/> model).
/// </summary>
public class ThreatRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_RoundTripsConvertedCollections()
    {
        using var database = SqliteTestDatabase.Create();

        using (var context = database.CreateContext())
        {
            var repository = new ThreatRepository(context);
            await repository.AddAsync(TestData.NewThreat(id: "threat-1", tags: new List<string> { "ssh", "brute-force" }));
        }

        using (var context = database.CreateContext())
        {
            var repository = new ThreatRepository(context);
            var loaded = await repository.GetByIdAsync("threat-1");

            Assert.NotNull(loaded);
            Assert.Equal(new[] { "ssh", "brute-force" }, loaded!.Tags);
            Assert.Equal(ThreatSeverity.High, loaded.Severity);
        }
    }

    [Fact]
    public async Task GetAllAndCount_ReturnAllRows()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new ThreatRepository(context);

        await repository.AddAsync(TestData.NewThreat(id: "threat-1"));
        await repository.AddAsync(TestData.NewThreat(id: "threat-2"));

        Assert.Equal(2, await repository.CountAsync());
        Assert.Equal(2, (await repository.GetAllAsync()).Count);
    }

    [Fact]
    public async Task Filters_WorkAgainstSqlite()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new ThreatRepository(context);

        await repository.AddAsync(TestData.NewThreat(id: "threat-1", severity: ThreatSeverity.Critical, status: ThreatStatus.New, targetId: "host-1"));
        await repository.AddAsync(TestData.NewThreat(id: "threat-2", severity: ThreatSeverity.Low, status: ThreatStatus.Contained, targetId: "host-1"));
        await repository.AddAsync(TestData.NewThreat(id: "threat-3", severity: ThreatSeverity.Low, status: ThreatStatus.New, targetId: "host-2"));

        Assert.Single(await repository.GetBySeverityAsync(ThreatSeverity.Critical));
        Assert.Equal(2, (await repository.GetByStatusAsync(ThreatStatus.New)).Count);
        Assert.Equal(2, (await repository.GetByTargetIdAsync("host-1")).Count);
    }

    [Fact]
    public async Task UpdateAndDelete_ModifyPersistedRows()
    {
        using var database = SqliteTestDatabase.Create();
        using (var context = database.CreateContext())
        {
            var repository = new ThreatRepository(context);
            await repository.AddAsync(TestData.NewThreat(id: "threat-1", name: "Original"));
        }

        using (var context = database.CreateContext())
        {
            var repository = new ThreatRepository(context);
            var existing = await repository.GetByIdAsync("threat-1");
            Assert.NotNull(existing);

            var updated = TestData.NewThreat(id: "threat-1", name: "Renamed", severity: ThreatSeverity.Critical);
            await repository.UpdateAsync(updated);
        }

        using (var context = database.CreateContext())
        {
            var repository = new ThreatRepository(context);
            var loaded = await repository.GetByIdAsync("threat-1");
            Assert.Equal("Renamed", loaded!.Name);
            Assert.Equal(ThreatSeverity.Critical, loaded.Severity);
        }

        using (var context = database.CreateContext())
        {
            var repository = new ThreatRepository(context);
            Assert.True(await repository.DeleteAsync("threat-1"));
            Assert.False(await repository.DeleteAsync("threat-1"));
            Assert.Equal(0, await repository.CountAsync());
        }
    }
}

public class VulnerabilityRepositoryTests
{
    [Fact]
    public async Task CrudAndFilters_Work()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new VulnerabilityRepository(context);

        await repository.AddAsync(TestData.NewVulnerability(id: "vuln-1", severity: VulnerabilitySeverity.Critical, status: VulnerabilityStatus.Open, targetId: "host-1"));
        await repository.AddAsync(TestData.NewVulnerability(id: "vuln-2", severity: VulnerabilitySeverity.Low, status: VulnerabilityStatus.Fixed, targetId: "host-2"));

        Assert.Equal(2, await repository.CountAsync());
        Assert.Single(await repository.GetBySeverityAsync(VulnerabilitySeverity.Critical));
        Assert.Single(await repository.GetByStatusAsync(VulnerabilityStatus.Fixed));
        Assert.Single(await repository.GetByTargetIdAsync("host-1"));

        var loaded = await repository.GetByIdAsync("vuln-1");
        Assert.NotNull(loaded);
        Assert.Equal("CVE-2026-0001", loaded!.CveId);
        Assert.Single(loaded.References);

        await repository.UpdateAsync(TestData.NewVulnerability(id: "vuln-1", status: VulnerabilityStatus.Fixed));
        Assert.Single(await repository.GetByStatusAsync(VulnerabilityStatus.Fixed));

        Assert.True(await repository.DeleteAsync("vuln-2"));
        Assert.Equal(1, await repository.CountAsync());
    }
}

public class IncidentRepositoryTests
{
    [Fact]
    public async Task CrudAndFilters_Work()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new IncidentRepository(context);

        await repository.AddAsync(TestData.NewIncident(id: "inc-1", severity: IncidentSeverity.Critical, status: IncidentStatus.New, relatedThreatIds: new List<string> { "threat-1" }));
        await repository.AddAsync(TestData.NewIncident(id: "inc-2", severity: IncidentSeverity.Low, status: IncidentStatus.Resolved));

        Assert.Equal(2, await repository.CountAsync());
        Assert.Single(await repository.GetBySeverityAsync(IncidentSeverity.Critical));
        Assert.Single(await repository.GetByStatusAsync(IncidentStatus.Resolved));
        Assert.Equal(2, (await repository.GetAllAsync()).Count);

        var loaded = await repository.GetByIdAsync("inc-1");
        Assert.NotNull(loaded);
        Assert.Equal(new[] { "threat-1" }, loaded!.RelatedThreatIds);

        Assert.True(await repository.DeleteAsync("inc-1"));
        Assert.Null(await repository.GetByIdAsync("inc-1"));
    }
}

public class EvidenceRepositoryTests
{
    [Fact]
    public async Task CrudAndFilters_Work()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new EvidenceRepository(context);

        await repository.AddAsync(TestData.NewEvidence(id: "evidence-1", type: EvidenceType.Log, status: EvidenceStatus.New));
        await repository.AddAsync(TestData.NewEvidence(id: "evidence-2", type: EvidenceType.Screenshot, status: EvidenceStatus.Reviewed));

        Assert.Equal(2, await repository.CountAsync());
        Assert.Single(await repository.GetByTypeAsync(EvidenceType.Log));
        Assert.Single(await repository.GetByStatusAsync(EvidenceStatus.Reviewed));

        var loaded = await repository.GetByIdAsync("evidence-1");
        Assert.NotNull(loaded);
        Assert.Equal(EvidenceClassification.Internal, loaded!.Classification);
        Assert.Single(loaded.Tags);
    }
}

public class HostRepositoryTests
{
    [Fact]
    public async Task CrudAndFilters_Work()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new HostRepository(context);

        await repository.AddAsync(TestData.NewHost(id: "host-1", type: AssetType.Server, status: AssetStatus.Active));
        await repository.AddAsync(TestData.NewHost(id: "host-2", type: AssetType.Workstation, status: AssetStatus.Inactive));

        Assert.Equal(2, await repository.CountAsync());
        Assert.Single(await repository.GetByTypeAsync(AssetType.Server));
        Assert.Single(await repository.GetByStatusAsync(AssetStatus.Inactive));

        var loaded = await repository.GetByIdAsync("host-1");
        Assert.NotNull(loaded);
        Assert.Equal("Edge VPS", loaded!.Name);
        Assert.Equal(OperatingSystemType.Linux, loaded.OperatingSystem);

        await repository.UpdateAsync(TestData.NewHost(id: "host-1", status: AssetStatus.Maintenance));
        using var readContext = database.CreateContext();
        var reloaded = await new HostRepository(readContext).GetByIdAsync("host-1");
        Assert.Equal(AssetStatus.Maintenance, reloaded!.Status);
    }
}

public class NetworkRepositoryTests
{
    [Fact]
    public async Task Crud_Works()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new NetworkRepository(context);

        await repository.AddAsync(TestData.NewNetwork(id: "net-1"));
        Assert.Equal(1, await repository.CountAsync());

        var loaded = await repository.GetByIdAsync("net-1");
        Assert.NotNull(loaded);
        Assert.Equal("192.168.1.0/24", loaded!.Cidr);
        Assert.Equal(new[] { "vlan-10" }, loaded.VlanIds);

        await repository.UpdateAsync(TestData.NewNetwork(id: "net-1", name: "Renamed LAN"));
        Assert.Equal("Renamed LAN", (await repository.GetByIdAsync("net-1"))!.Name);

        Assert.True(await repository.DeleteAsync("net-1"));
        Assert.Empty(await repository.GetAllAsync());
    }
}

public class UserRepositoryTests
{
    [Fact]
    public async Task CrudAndLookups_Work()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new UserRepository(context);

        await repository.AddAsync(TestData.NewUser(id: "user-1", username: "neo", email: "neo@socpit.local", role: UserRole.SuperAdministrator));
        await repository.AddAsync(TestData.NewUser(id: "user-2", username: "analyst", email: "analyst@socpit.local", role: UserRole.Analyst));

        Assert.Equal(2, await repository.CountAsync());
        Assert.NotNull(await repository.GetByUsernameAsync("neo"));
        Assert.Null(await repository.GetByUsernameAsync("ghost"));
        Assert.NotNull(await repository.GetByEmailAsync("analyst@socpit.local"));
        Assert.Null(await repository.GetByEmailAsync("ghost@socpit.local"));
        Assert.Single(await repository.GetByRoleAsync(UserRole.SuperAdministrator));

        var loaded = await repository.GetByIdAsync("user-1");
        Assert.NotNull(loaded);
        Assert.Equal(new[] { "perm-read" }, loaded!.PermissionIds);

        Assert.True(await repository.DeleteAsync("user-2"));
        Assert.Equal(1, await repository.CountAsync());
    }
}

public class AuditLogRepositoryTests
{
    [Fact]
    public async Task AddAndQuery_Work()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();
        var repository = new AuditLogRepository(context);

        await repository.AddAsync(TestData.NewAuditLog(id: "audit-1", entityType: "Threat", entityId: "threat-1", userId: "analyst-1"));
        await repository.AddAsync(TestData.NewAuditLog(id: "audit-2", entityType: "Host", entityId: "host-1", userId: "analyst-2"));

        Assert.Equal(2, await repository.CountAsync());
        Assert.Single(await repository.GetByEntityAsync("Threat", "threat-1"));
        Assert.Single(await repository.GetByUserAsync("analyst-2"));
        Assert.Equal(2, (await repository.GetByDateRangeAsync(TestData.Now.AddHours(-1), TestData.Now.AddHours(1))).Count);
        Assert.Empty(await repository.GetByDateRangeAsync(TestData.Now.AddDays(-5), TestData.Now.AddDays(-4)));
    }
}
