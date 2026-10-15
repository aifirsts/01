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

public class UserServiceTests
{
    private static (UserService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new UserService(
            unitOfWork.UsersRepo,
            unitOfWork.AuditLogsRepo,
            NullLogger<UserService>.Instance);

        return (service, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_PersistsUserAndAuditEntry()
    {
        var (service, unitOfWork) = CreateService();

        var user = await service.CreateAsync(
            username: "neo",
            email: "neo@socpit.local",
            fullName: "NEO",
            role: UserRole.SuperAdministrator,
            status: UserStatus.Active,
            authenticationType: AuthenticationType.MultiFactor,
            tags: new List<string> { "owner" },
            passwordHash: "hash",
            department: "Security Operations",
            phone: "+65-0000-0000",
            permissionIds: new List<string> { "perm-all" },
            createdBy: "system");

        Assert.Single(unitOfWork.UsersRepo.Store);
        Assert.Equal("neo", user.Username);
        Assert.Equal(new[] { "perm-all" }, user.PermissionIds);

        var audit = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("User", audit.EntityType);
        Assert.Contains("neo@socpit.local", audit.Details);
    }

    [Fact]
    public async Task Lookups_FindByUsernameEmailAndRole()
    {
        var (service, unitOfWork) = CreateService();

        unitOfWork.UsersRepo.Seed(
            TestData.NewUser(id: "user-1", username: "neo", email: "neo@socpit.local", role: UserRole.SuperAdministrator),
            TestData.NewUser(id: "user-2", username: "analyst", email: "analyst@socpit.local", role: UserRole.Analyst));

        Assert.NotNull(await service.GetByUsernameAsync("neo"));
        Assert.NotNull(await service.GetByEmailAsync("analyst@socpit.local"));
        Assert.Null(await service.GetByUsernameAsync("ghost"));
        Assert.Null(await service.GetByEmailAsync("ghost@socpit.local"));
        Assert.Single(await service.GetByRoleAsync(UserRole.SuperAdministrator));
        Assert.Equal(2, await service.CountAsync());
        Assert.Equal(2, (await service.GetAllAsync()).Count);
    }

    [Fact]
    public async Task UpdateAsync_ChangesRoleAndWritesAudit()
    {
        var (service, unitOfWork) = CreateService();
        var user = await service.CreateAsync("analyst", "analyst@socpit.local", "Analyst", UserRole.Analyst, UserStatus.Active, AuthenticationType.Local, new List<string>());

        var updated = await service.UpdateAsync(user.Id, role: UserRole.Investigator, department: "Threat Intel", updatedBy: "NEO");

        Assert.Equal(UserRole.Investigator, updated.Role);
        Assert.Equal("Threat Intel", updated.Department);
        Assert.Equal("analyst", updated.Username);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Update" && a.UserId == "NEO");
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync("missing", role: UserRole.Analyst));
    }

    [Fact]
    public async Task LockAsync_SetsLockedStatusAndExpiry()
    {
        var (service, unitOfWork) = CreateService();
        var user = await service.CreateAsync("analyst", "analyst@socpit.local", "Analyst", UserRole.Analyst, UserStatus.Active, AuthenticationType.Local, new List<string>());

        var lockedUntil = TestData.Now.AddDays(7);
        var locked = await service.LockAsync(user.Id, lockedUntil, lockedBy: "NEO");

        Assert.Equal(UserStatus.Locked, locked.Status);
        Assert.Equal(lockedUntil, locked.LockedUntil);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Lock" && a.UserId == "NEO");
    }

    [Fact]
    public async Task LockAsync_DefaultsToThirtyDayLock()
    {
        var (service, _) = CreateService();
        var user = await service.CreateAsync("analyst", "analyst@socpit.local", "Analyst", UserRole.Analyst, UserStatus.Active, AuthenticationType.Local, new List<string>());

        var locked = await service.LockAsync(user.Id);

        Assert.Equal(UserStatus.Locked, locked.Status);
        Assert.NotNull(locked.LockedUntil);
        Assert.True(locked.LockedUntil > DateTimeOffset.UtcNow.AddDays(29));
    }

    [Fact]
    public async Task LockAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.LockAsync("missing"));
    }

    [Fact]
    public async Task UnlockAsync_RestoresActiveStatusAndClearsCounters()
    {
        var (service, unitOfWork) = CreateService();
        var user = await service.CreateAsync("analyst", "analyst@socpit.local", "Analyst", UserRole.Analyst, UserStatus.Active, AuthenticationType.Local, new List<string>());
        await service.LockAsync(user.Id, TestData.Now.AddDays(1), "NEO");

        var unlocked = await service.UnlockAsync(user.Id, unlockedBy: "NEO");

        Assert.Equal(UserStatus.Active, unlocked.Status);
        Assert.Null(unlocked.LockedUntil);
        Assert.Equal(0, unlocked.FailedLoginAttempts);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Unlock");
    }

    [Fact]
    public async Task UnlockAsync_UnknownId_Throws()
    {
        var (service, _) = CreateService();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UnlockAsync("missing"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesUserAndAudits()
    {
        var (service, unitOfWork) = CreateService();
        var user = await service.CreateAsync("analyst", "analyst@socpit.local", "Analyst", UserRole.Analyst, UserStatus.Active, AuthenticationType.Local, new List<string>());

        Assert.True(await service.DeleteAsync(user.Id, deletedBy: "NEO"));
        Assert.Empty(unitOfWork.UsersRepo.Store);
        Assert.Contains(unitOfWork.AuditLogsRepo.Entries, a => a.Action == "Delete");
        Assert.False(await service.DeleteAsync("missing"));
    }
}

public class AuditServiceTests
{
    private static (AuditService Service, FakeUnitOfWork UnitOfWork) CreateService()
    {
        var unitOfWork = new FakeUnitOfWork();
        var service = new AuditService(unitOfWork.AuditLogsRepo, NullLogger<AuditService>.Instance);
        return (service, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_StoresAuditEntryWithAllFields()
    {
        var (service, unitOfWork) = CreateService();

        var entry = await service.CreateAsync(
            action: "Update",
            entityType: "Threat",
            entityId: "threat-1",
            userId: "analyst-1",
            details: "Severity raised",
            oldValue: "Low",
            newValue: "Critical",
            ipAddress: "203.0.113.7",
            userAgent: "SOCpit/2026.10.15");

        Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("Update", entry.Action);
        Assert.Equal("Low", entry.OldValue);
        Assert.Equal("Critical", entry.NewValue);
        Assert.True(entry.Success);
        Assert.Null(entry.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_FailedOperation_IsRecordedWithErrorMessage()
    {
        var (service, _) = CreateService();

        var entry = await service.CreateAsync(
            action: "Login",
            entityType: "User",
            entityId: "user-1",
            userId: "user-1",
            success: false,
            errorMessage: "Invalid credentials");

        Assert.False(entry.Success);
        Assert.Equal("Invalid credentials", entry.ErrorMessage);
    }

    [Fact]
    public async Task LogActionAsync_DelegatesToCreateAsync()
    {
        var (service, unitOfWork) = CreateService();

        await service.LogActionAsync("Export", "Report", "report-1", "NEO", details: "Weekly export");

        var stored = Assert.Single(unitOfWork.AuditLogsRepo.Entries);
        Assert.Equal("Export", stored.Action);
        Assert.Equal("report-1", stored.EntityId);
    }

    [Fact]
    public async Task QueryMethods_FilterByEntityUserAndRange()
    {
        var (service, unitOfWork) = CreateService();

        var earlier = AuditLog.Create("audit-1", "Create", "Threat", "threat-1", "analyst-1", TestData.Now.AddHours(-2), "Created threat");
        var later = AuditLog.Create("audit-2", "Delete", "Threat", "threat-1", "analyst-2", TestData.Now, "Deleted threat");
        var other = AuditLog.Create("audit-3", "Create", "Host", "host-1", "analyst-1", TestData.Now, "Created host");

        await unitOfWork.AuditLogsRepo.AddAsync(earlier);
        await unitOfWork.AuditLogsRepo.AddAsync(later);
        await unitOfWork.AuditLogsRepo.AddAsync(other);

        Assert.Equal(3, await service.CountAsync());
        Assert.Equal(2, (await service.GetByEntityAsync("Threat", "threat-1")).Count);
        Assert.Equal(2, (await service.GetByUserAsync("analyst-1")).Count);
        Assert.Equal(2, (await service.GetByDateRangeAsync(TestData.Now.AddHours(-1), TestData.Now.AddHours(1))).Count);
    }
}
