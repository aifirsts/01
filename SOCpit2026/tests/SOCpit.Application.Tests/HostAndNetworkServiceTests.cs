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

public class HostServiceTests
{
    private static (HostService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new HostService(
            unitOfWork.HostsRepo,
            unitOfWork.AuditLogsRepo,
            NullLogger<HostService>.Instance);

        return (service, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_PersistsHostAndAuditEntry()
    {
        var (service, unitOfWork) = CreateService();

        var host = await service.CreateAsync(
            name: "Edge VPS",
            ipAddress: "203.0.113.10",
            macAddress: "00:1A:2B:3C:4D:5E",
            type: AssetType.Server,
            status: AssetStatus.Active,
            operatingSystem: OperatingSystemType.Linux,
            tags: new List<string> { "edge" },
            osVersion: "Ubuntu 24.04",
            hostname: "edge-01",
            domain: "neo.infra",
            location: "Cloud EU",
            owner: "NEO",
            createdBy: "NEO");

        Assert.Single(unitOfWork.HostsRepo.Store);
        Assert.Equal("edge-01", host.Hostname);

        var audit = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("Host", audit.EntityType);
        Assert.Contains("203.0.113.10", audit.Details);
    }

    [Fact]
    public async Task QueryMethods_FilterByTypeAndStatus()
    {
        var (service, unitOfWork) = CreateService();

        unitOfWork.HostsRepo.Seed(
            TestData.NewHost(id: "host-1", type: AssetType.Server, status: AssetStatus.Active),
            TestData.NewHost(id: "host-2", type: AssetType.Workstation, status: AssetStatus.Active),
            TestData.NewHost(id: "host-3", type: AssetType.Server, status: AssetStatus.Inactive));

        Assert.Equal(3, await service.CountAsync());
        Assert.Equal(2, (await service.GetByTypeAsync(AssetType.Server)).Count);
        Assert.Equal(2, (await service.GetByStatusAsync(AssetStatus.Active)).Count);
        Assert.Equal(3, (await service.GetAllAsync()).Count);
        Assert.NotNull(await service.GetByIdAsync("host-1"));
        Assert.Null(await service.GetByIdAsync("host-missing"));
    }

    [Fact]
    public async Task UpdateAsync_AppliesPartialChanges()
    {
        var (service, _) = CreateService();
        var host = await service.CreateAsync("Edge VPS", "203.0.113.10", "00:1A:2B:3C:4D:5E", AssetType.Server, AssetStatus.Active, OperatingSystemType.Linux, new List<string>());

        var updated = await service.UpdateAsync(host.Id, status: AssetStatus.Maintenance, location: "Cloud SG", owner: "SOC", updatedBy: "NEO");

        Assert.Equal(AssetStatus.Maintenance, updated.Status);
        Assert.Equal("Cloud SG", updated.Location);
        Assert.Equal("SOC", updated.Owner);
        Assert.Equal("Edge VPS", updated.Name);
        Assert.Equal("203.0.113.10", updated.IpAddress);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync("missing", status: AssetStatus.Inactive));
    }

    [Fact]
    public async Task DeleteAsync_RemovesHostAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var host = await service.CreateAsync("Edge VPS", "203.0.113.10", "00:1A:2B:3C:4D:5E", AssetType.Server, AssetStatus.Active, OperatingSystemType.Linux, new List<string>());

        Assert.True(await service.DeleteAsync(host.Id, deletedBy: "NEO"));
        Assert.Empty(unitOfWork.HostsRepo.Store);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Delete");
        Assert.False(await service.DeleteAsync("missing"));
    }
}

public class NetworkServiceTests
{
    private static (NetworkService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new NetworkService(
            unitOfWork.NetworksRepo,
            unitOfWork.HostsRepo,
            unitOfWork.AuditLogsRepo,
            NullLogger<NetworkService>.Instance);

        return (service, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_PersistsNetworkAndAuditEntry()
    {
        var (service, unitOfWork) = CreateService();

        var network = await service.CreateAsync(
            name: "Internal LAN",
            cidr: "192.168.1.0/24",
            description: "Primary internal network",
            type: "Internal",
            tags: new List<string> { "internal" },
            vlanIds: new List<string> { "vlan-10" },
            subnetIds: new List<string> { "subnet-10" },
            createdBy: "NEO");

        Assert.Single(unitOfWork.NetworksRepo.Store);
        Assert.Equal("192.168.1.0/24", network.Cidr);
        Assert.Equal(new[] { "vlan-10" }, network.VlanIds);

        var audit = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("Network", audit.EntityType);
        Assert.Contains("192.168.1.0/24", audit.Details);
    }

    [Fact]
    public async Task GetAndCount_ReturnStoredNetworks()
    {
        var (service, unitOfWork) = CreateService();
        unitOfWork.NetworksRepo.Seed(TestData.NewNetwork(id: "net-1"), TestData.NewNetwork(id: "net-2", name: "DMZ", cidr: "10.10.0.0/24"));

        Assert.Equal(2, await service.CountAsync());
        Assert.Equal(2, (await service.GetAllAsync()).Count);
        Assert.NotNull(await service.GetByIdAsync("net-1"));
        Assert.Null(await service.GetByIdAsync("net-missing"));
    }

    [Fact]
    public async Task UpdateAsync_AppliesPartialChanges()
    {
        var (service, _) = CreateService();
        var network = await service.CreateAsync("Internal LAN", "192.168.1.0/24", "Primary", "Internal", new List<string>());

        var updated = await service.UpdateAsync(network.Id, name: "Renamed LAN", description: "Updated", updatedBy: "NEO");

        Assert.Equal("Renamed LAN", updated.Name);
        Assert.Equal("Updated", updated.Description);
        Assert.Equal("192.168.1.0/24", updated.Cidr);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync("missing", name: "x"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesNetworkAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var network = await service.CreateAsync("Internal LAN", "192.168.1.0/24", "Primary", "Internal", new List<string>());

        Assert.True(await service.DeleteAsync(network.Id, deletedBy: "NEO"));
        Assert.Empty(unitOfWork.NetworksRepo.Store);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Delete");
        Assert.False(await service.DeleteAsync("missing"));
    }
}
