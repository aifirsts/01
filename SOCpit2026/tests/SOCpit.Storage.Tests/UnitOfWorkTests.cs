using System;
using System.Threading.Tasks;
using SOCpit.Core;
using SOCpit.Storage;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Storage.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public void AllRepositories_AreExposedAndNonNull()
    {
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        Assert.NotNull(unitOfWork.Threats);
        Assert.NotNull(unitOfWork.Vulnerabilities);
        Assert.NotNull(unitOfWork.Incidents);
        Assert.NotNull(unitOfWork.Evidence);
        Assert.NotNull(unitOfWork.Hosts);
        Assert.NotNull(unitOfWork.Networks);
        Assert.NotNull(unitOfWork.Users);
        Assert.NotNull(unitOfWork.AuditLogs);
    }

    [Fact]
    public void Repositories_AreCachedPerUnitOfWork()
    {
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        Assert.Same(unitOfWork.Threats, unitOfWork.Threats);
        Assert.Same(unitOfWork.Hosts, unitOfWork.Hosts);
        Assert.Same(unitOfWork.AuditLogs, unitOfWork.AuditLogs);
    }

    [Fact]
    public async Task CommitAsync_PersistsTrackedChanges()
    {
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        await unitOfWork.Hosts.AddAsync(TestData.NewHost(id: "host-1"));

        var affected = await unitOfWork.CommitAsync();
        Assert.True(affected >= 0);
        Assert.Equal(1, await unitOfWork.Hosts.CountAsync());
    }

    [Fact]
    public async Task Repositories_ShareTheSameDatabase()
    {
        using var database = SqliteTestDatabase.Create();
        using var unitOfWork = database.CreateUnitOfWork();

        await unitOfWork.Threats.AddAsync(TestData.NewThreat(id: "threat-1"));
        await unitOfWork.Vulnerabilities.AddAsync(TestData.NewVulnerability(id: "vuln-1"));
        await unitOfWork.Incidents.AddAsync(TestData.NewIncident(id: "inc-1"));
        await unitOfWork.Evidence.AddAsync(TestData.NewEvidence(id: "evidence-1"));
        await unitOfWork.Hosts.AddAsync(TestData.NewHost(id: "host-1"));
        await unitOfWork.Networks.AddAsync(TestData.NewNetwork(id: "net-1"));
        await unitOfWork.Users.AddAsync(TestData.NewUser(id: "user-1"));
        await unitOfWork.AuditLogs.AddAsync(TestData.NewAuditLog(id: "audit-1"));

        Assert.Equal(1, await unitOfWork.Threats.CountAsync());
        Assert.Equal(1, await unitOfWork.Vulnerabilities.CountAsync());
        Assert.Equal(1, await unitOfWork.Incidents.CountAsync());
        Assert.Equal(1, await unitOfWork.Evidence.CountAsync());
        Assert.Equal(1, await unitOfWork.Hosts.CountAsync());
        Assert.Equal(1, await unitOfWork.Networks.CountAsync());
        Assert.Equal(1, await unitOfWork.Users.CountAsync());
        Assert.Equal(1, await unitOfWork.AuditLogs.CountAsync());
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        using var database = SqliteTestDatabase.Create();
        var unitOfWork = database.CreateUnitOfWork();

        unitOfWork.Dispose();
        unitOfWork.Dispose();
    }

    [Fact]
    public async Task DisposeAsync_IsIdempotent()
    {
        using var database = SqliteTestDatabase.Create();
        var unitOfWork = database.CreateUnitOfWork();

        await unitOfWork.DisposeAsync();
        await unitOfWork.DisposeAsync();
    }

    [Fact]
    public void Constructor_NullContext_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new UnitOfWork(null!));
    }
}
