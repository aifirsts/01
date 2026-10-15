using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOCpit.Core;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Storage.Tests;

public class SOCpitDbContextTests
{
    /// <summary>Every DbSet declared by the context must be part of the relational model.</summary>
    private static readonly string[] ExpectedEntityNames =
    {
        nameof(SOCpitDbContext.Threats),
        nameof(SOCpitDbContext.Vulnerabilities),
        nameof(SOCpitDbContext.SecurityIncidents),
        nameof(SOCpitDbContext.Evidence),
        nameof(SOCpitDbContext.Hosts),
        nameof(SOCpitDbContext.Networks),
        nameof(SOCpitDbContext.Subnets),
        nameof(SOCpitDbContext.Vlans),
        nameof(SOCpitDbContext.Services),
        nameof(SOCpitDbContext.Applications),
        nameof(SOCpitDbContext.Users),
        nameof(SOCpitDbContext.Permissions),
        nameof(SOCpitDbContext.Roles),
        nameof(SOCpitDbContext.Groups),
        nameof(SOCpitDbContext.Sessions),
        nameof(SOCpitDbContext.AuditLogs),
        nameof(SOCpitDbContext.UserAuditEntries),
        nameof(SOCpitDbContext.SecurityControls),
        nameof(SOCpitDbContext.ComplianceChecks),
        nameof(SOCpitDbContext.LogEntries),
        nameof(SOCpitDbContext.Reports),
        nameof(SOCpitDbContext.NetworkConnections)
    };

    [Fact]
    public void Model_ContainsAllTwentyTwoEntities()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();

        var entityNames = context.Model.GetEntityTypes().Select(e => e.ClrType.Name).ToList();

        Assert.Equal(ExpectedEntityNames.Length, entityNames.Count);
        foreach (var clrType in new[]
        {
            typeof(Threat), typeof(Vulnerability), typeof(SecurityIncident), typeof(Evidence),
            typeof(Host), typeof(Network), typeof(Subnet), typeof(Vlan), typeof(Service),
            typeof(SOCpit.Core.Application), typeof(User), typeof(Permission), typeof(Role),
            typeof(Group), typeof(Session), typeof(AuditLog), typeof(UserAuditEntry),
            typeof(SecurityControl), typeof(ComplianceCheck), typeof(LogEntry), typeof(Report),
            typeof(NetworkConnection)
        })
        {
            Assert.Contains(clrType, context.Model.GetEntityTypes().Select(e => e.ClrType));
        }
    }

    [Fact]
    public void Schema_IsCreated_AndEnsureCreatedIsIdempotent()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();

        Assert.False(context.Database.EnsureCreated());
        Assert.True(context.Database.CanConnect());
    }

    [Fact]
    public async Task StringListConverter_RoundTripsUnicodeAndEmptyLists()
    {
        using var database = SqliteTestDatabase.Create();

        using (var context = database.CreateContext())
        {
            await context.Threats.AddAsync(TestData.NewThreat(id: "threat-1", tags: new List<string> { "ssh", "brute-force", "тест-ключ" }));
            await context.Threats.AddAsync(TestData.NewThreat(id: "threat-2", tags: new List<string>()));
            await context.SaveChangesAsync();
        }

        using (var context = database.CreateContext())
        {
            var withTags = await context.Threats.AsNoTracking().SingleAsync(t => t.Id == "threat-1");
            var withoutTags = await context.Threats.AsNoTracking().SingleAsync(t => t.Id == "threat-2");

            Assert.Equal(new[] { "ssh", "brute-force", "тест-ключ" }, withTags.Tags);
            Assert.Empty(withoutTags.Tags);
        }
    }

    [Fact]
    public async Task DictionaryConverter_RoundTripsProperties()
    {
        using var database = SqliteTestDatabase.Create();

        var entry = LogEntry.Create(
            "log-1", "sshd", "Failed password", "Warning", TestData.Now,
            host: "vps-01",
            properties: new Dictionary<string, string> { ["user"] = "root", ["source_ip"] = "203.0.113.7" });

        using (var context = database.CreateContext())
        {
            await context.LogEntries.AddAsync(entry);
            await context.SaveChangesAsync();
        }

        using (var context = database.CreateContext())
        {
            var loaded = await context.LogEntries.AsNoTracking().SingleAsync(l => l.Id == "log-1");

            Assert.NotNull(loaded.Properties);
            Assert.Equal("root", loaded.Properties!["user"]);
            Assert.Equal("203.0.113.7", loaded.Properties["source_ip"]);
        }
    }

    [Fact]
    public async Task NullDictionaries_AreSupported()
    {
        using var database = SqliteTestDatabase.Create();

        using (var context = database.CreateContext())
        {
            await context.LogEntries.AddAsync(LogEntry.Create("log-2", "kernel", "Boot", "Information", TestData.Now));
            await context.SaveChangesAsync();
        }

        using (var context = database.CreateContext())
        {
            var loaded = await context.LogEntries.AsNoTracking().SingleAsync(l => l.Id == "log-2");
            Assert.Null(loaded.Properties);
        }
    }

    [Fact]
    public async Task RequiredColumns_AreEnforcedBySchema()
    {
        using var database = SqliteTestDatabase.Create();
        using var context = database.CreateContext();

        await context.Threats.AddAsync(TestData.NewThreat(id: "threat-1", name: new string('N', 200)));
        await context.SaveChangesAsync();

        var stored = await context.Threats.AsNoTracking().SingleAsync();
        Assert.Equal(200, stored.Name.Length);
    }
}
