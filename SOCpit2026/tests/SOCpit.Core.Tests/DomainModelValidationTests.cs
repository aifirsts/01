using System;
using System.Collections.Generic;
using SOCpit.Core;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Core.Tests;

/// <summary>
/// Construction-time invariants of the SOCpit domain model.
/// </summary>
public class DomainModelValidationTests
{
    private static readonly DateTimeOffset Now = TestData.Now;

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Threat_Create_EmptyId_Throws(string id)
    {
        Assert.Throws<ArgumentException>(() => TestData.NewThreat(id: id));
    }

    [Fact]
    public void Threat_Create_IdLongerThan80Chars_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestData.NewThreat(id: new string('a', 81)));
    }

    [Fact]
    public void Threat_Create_IdWithInvalidCharacters_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestData.NewThreat(id: "bad id!"));
    }

    [Fact]
    public void Threat_Create_WhitespaceName_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestData.NewThreat(name: "  "));
    }

    [Fact]
    public void Threat_Create_TrimsName()
    {
        var threat = TestData.NewThreat(name: "  Padded Name  ");
        Assert.Equal("Padded Name", threat.Name);
    }

    [Fact]
    public void Threat_Create_DefaultsDetectedAtToProvidedTimestamp()
    {
        var threat = TestData.NewThreat();
        Assert.Equal(TestData.Now, threat.DetectedAt);
    }

    [Fact]
    public void Vulnerability_Create_DefaultsReferencesToEmptyList()
    {
        var vulnerability = Vulnerability.Create(
            id: "vuln-x",
            cveId: "CVE-2026-9999",
            name: "Name",
            description: "Description",
            severity: VulnerabilitySeverity.Low,
            status: VulnerabilityStatus.Open,
            targetId: "host-1",
            publishedAt: Now);

        Assert.Empty(vulnerability.References);
        Assert.Equal("CVE-2026-9999", vulnerability.CveId);
    }

    [Fact]
    public void Vulnerability_Create_NullCveId_FallsBackToNotAvailable()
    {
        var vulnerability = Vulnerability.Create(
            id: "vuln-y",
            cveId: null!,
            name: "Name",
            description: "Description",
            severity: VulnerabilitySeverity.Low,
            status: VulnerabilityStatus.Open,
            targetId: "host-1",
            publishedAt: Now);

        Assert.Equal("N/A", vulnerability.CveId);
    }

    [Fact]
    public void SecurityIncident_Create_DefaultsRelatedCollectionsToEmpty()
    {
        var incident = TestData.NewIncident();
        Assert.Empty(incident.RelatedThreatIds);
        Assert.Empty(incident.RelatedVulnerabilityIds);
        Assert.Null(incident.ResolvedAt);
    }

    [Fact]
    public void SecurityControl_Create_NonPositiveFrequencyFallsBackToDefault()
    {
        var control = SecurityControl.Create(
            id: "control-1",
            name: "MFA enforced",
            description: "All privileged accounts use MFA",
            type: SecurityControlType.Preventive,
            category: "Identity",
            required: true,
            scope: "all-users",
            implementation: "Entra ID conditional access",
            frequencyDays: 0);

        Assert.Equal(30, control.FrequencyDays);
    }

    [Fact]
    public void Node_Create_ValidatesIdentifierAndRequiredFields()
    {
        var node = Node.Create("node-1", NodeType.Device, "Laptop", "NEO", "home", Sensitivity.Internal, Availability.Up);
        Assert.Equal("node-1", node.Id);
        Assert.Equal("Laptop", node.Name);

        Assert.Throws<ArgumentException>(() => Node.Create("bad id", NodeType.Device, "Laptop", "NEO", "home", Sensitivity.Internal, Availability.Up));
        Assert.Throws<ArgumentException>(() => Node.Create("node-2", NodeType.Device, " ", "NEO", "home", Sensitivity.Internal, Availability.Up));
        Assert.Throws<ArgumentException>(() => Node.Create("node-3", NodeType.Device, "Laptop", "", "home", Sensitivity.Internal, Availability.Up));
        Assert.Throws<ArgumentException>(() => Node.Create("node-4", NodeType.Device, "Laptop", "NEO", " ", Sensitivity.Internal, Availability.Up));
    }

    [Fact]
    public void Node_Create_SetsTimestampsWhenNotProvided()
    {
        var node = Node.Create("node-ts", NodeType.Service, "API", "NEO", "dmz", Sensitivity.Sensitive, Availability.Up);
        Assert.NotNull(node.CreatedAt);
        Assert.NotNull(node.UpdatedAt);
    }

    [Fact]
    public void Edge_Create_ValidatesEndpoints()
    {
        var edge = Edge.Create("edge-1", "node-a", "node-b", EdgeKind.ConnectsTo, Criticality.High);
        Assert.Equal("node-a", edge.From);
        Assert.Equal("node-b", edge.To);

        Assert.Throws<ArgumentException>(() => Edge.Create("edge-2", "bad id", "node-b", EdgeKind.ConnectsTo, Criticality.High));
    }

    [Fact]
    public void Project_Create_AcceptsOnlyHttpsGitHubUrls()
    {
        var project = Project.Create("project-1", "node-1", "goal", "https://github.com/aifirsts/01", "stage", "next", new List<string> { "m1" });
        Assert.Equal("https://github.com/aifirsts/01", project.RepoUrl);

        Assert.Throws<ArgumentException>(() => Project.Create("project-2", "node-1", "goal", "https://gitlab.com/repo", "stage", "next", new List<string>()));
        Assert.Throws<ArgumentException>(() => Project.Create("project-3", "node-1", "goal", "http://github.com/repo", "stage", "next", new List<string>()));
    }

    [Fact]
    public void Project_Create_AllowsNullRepositoryUrl()
    {
        var project = Project.Create("project-4", "node-1", "goal", null, "stage", "next", new List<string>());
        Assert.Null(project.RepoUrl);
    }

    [Fact]
    public void Control_Create_ValidatesTtlRange()
    {
        Assert.Throws<ArgumentException>(() => Control.Create("ctrl-1", "node-1", "c", "s", "m", true, Criticality.Low, 0, ControlApplicability.Applicable, "r"));
        Assert.Throws<ArgumentException>(() => Control.Create("ctrl-2", "node-1", "c", "s", "m", true, Criticality.Low, 31536001, ControlApplicability.Applicable, "r"));

        var control = Control.Create("ctrl-3", "node-1", "c", "s", "m", true, Criticality.Low, 31536000, ControlApplicability.Applicable, "r");
        Assert.Equal(31536000, control.TtlSeconds);
    }

    [Fact]
    public void ControlEvidence_Create_PreservesProvenanceAndSupersedes()
    {
        var evidence = TestData.NewControlEvidence(id: "cev-1", supersedes: new List<string> { "cev-0" });
        Assert.Equal(EvidenceProvenance.AutomatedScan, evidence.Provenance);
        Assert.False(evidence.Revoked);
        Assert.Equal(new[] { "cev-0" }, evidence.Supersedes);
    }

    [Fact]
    public void Vlan_Create_ValidatesVlanIdRange()
    {
        var vlan = Vlan.Create("vlan-1", "net-1", 4095, "Mgmt", "Management VLAN", new List<string>());
        Assert.Equal(4095, vlan.VlanId);

        Assert.Throws<ArgumentException>(() => Vlan.Create("vlan-2", "net-1", 4096, "Mgmt", "Management VLAN", new List<string>()));
        Assert.Throws<ArgumentException>(() => Vlan.Create("vlan-3", "net-1", -1, "Mgmt", "Management VLAN", new List<string>()));
    }

    [Fact]
    public void NetworkConnection_Create_ClampsNegativeCountersAndValidatesPorts()
    {
        var connection = NetworkConnection.Create(
            "conn-1", "10.0.0.1", 443, "10.0.0.2", 55000, NetworkProtocol.Tcp,
            Now, bytesSent: -5, bytesReceived: -10);

        Assert.Equal(0, connection.BytesSent);
        Assert.Equal(0, connection.BytesReceived);

        Assert.Throws<ArgumentException>(() => NetworkConnection.Create("conn-2", "10.0.0.1", 70000, "10.0.0.2", 80, NetworkProtocol.Tcp, Now));
        Assert.Throws<ArgumentException>(() => NetworkConnection.Create("conn-3", "10.0.0.1", 80, "10.0.0.2", -1, NetworkProtocol.Tcp, Now));
    }

    [Fact]
    public void Host_Create_RequiresTagsAndCoreFields()
    {
        var host = TestData.NewHost();
        Assert.Equal("Edge VPS", host.Name);
        Assert.Single(host.Tags);

        Assert.Throws<ArgumentException>(() => Host.Create("host-1", "name", "1.1.1.1", "00:00:00:00:00:00", AssetType.Server, AssetStatus.Active, OperatingSystemType.Linux, Now, null!));
    }

    [Fact]
    public void Subnet_And_Service_Create_SetCollections()
    {
        var subnet = Subnet.Create("subnet-1", "net-1", "10.0.0.0/24", "DMZ subnet", "Perimeter subnet", new List<string> { "dmz" });
        Assert.Empty(subnet.HostIds);

        var service = Service.Create("svc-1", "SSH", "Remote shell", "RemoteAccess", new List<string> { "host-1" }, new List<int> { 22 }, "Running", new List<string> { "ssh" }, "9.6p1", "NEO");
        Assert.Equal(22, service.Ports[0]);
        Assert.Equal("9.6p1", service.Version);
    }

    [Fact]
    public void User_Create_DefaultsPermissionsAndClampsFailedLogins()
    {
        var user = User.Create(
            "user-1", "neo", "neo@socpit.local", "NEO", UserRole.SuperAdministrator, UserStatus.Active,
            AuthenticationType.MultiFactor, Now, new List<string>(), failedLoginAttempts: -3);

        Assert.Empty(user.PermissionIds);
        Assert.Equal(0, user.FailedLoginAttempts);

        Assert.Throws<ArgumentException>(() => User.Create("user-2", " ", "a@b.c", "Name", UserRole.Analyst, UserStatus.Active, AuthenticationType.Local, Now, new List<string>()));
    }

    [Fact]
    public void Permission_Role_Group_Session_Create_Work()
    {
        var permission = Permission.Create("perm-1", "threat:read", "Read threats", "Threats", "Threat", new List<string> { "read" }, isSystemPermission: true);
        Assert.True(permission.IsSystemPermission);

        var role = Role.Create("role-1", "Analyst", "SOC analyst role", new List<string> { "perm-1" });
        Assert.False(role.IsSystemRole);

        var group = Group.Create("group-1", "SOC Team", "Tier 2", new List<string> { "user-1" }, new List<string> { "role-1" }, new List<string> { "perm-1" });
        Assert.Single(group.UserIds);

        var session = Session.Create("session-1", "user-1", "token-value", Now, Now.AddHours(8), "203.0.113.7", "Mozilla/5.0");
        Assert.True(session.IsActive);
        Assert.Null(session.RefreshToken);
    }

    [Fact]
    public void AuditLog_LogEntry_Report_UserAuditEntry_Create_Work()
    {
        var auditLog = TestData.NewAuditLog(id: "audit-1", action: "Login", entityType: "User", entityId: "user-1", userId: "user-1");
        Assert.Equal("Login", auditLog.Action);
        Assert.True(auditLog.Success);
        Assert.Null(auditLog.ErrorMessage);

        var logEntry = LogEntry.Create("log-1", "sshd", "Failed password", "Warning", Now, host: "vps-01");
        Assert.Equal("sshd", logEntry.Source);

        var report = Report.Create("report-1", "Weekly SOC report", "Weekly", "content", Now, "NEO", @"C:\SOCpit2026\reports\weekly.html");
        Assert.Empty(report.Parameters);
        Assert.Null(report.TemplateId);

        var entry = UserAuditEntry.Create("ua-1", "user-1", "PasswordReset", Now, details: "self-service");
        Assert.True(entry.Success);
    }

    [Fact]
    public void Report_Create_RequiresGeneratedBy()
    {
        Assert.Throws<ArgumentException>(() => Report.Create("report-2", "Title", "Type", "content", Now, null!, @"C:\report.html"));
    }

    [Fact]
    public void Catalog_State_Workspace_DefaultToSchemaVersionOne()
    {
        var catalog = Catalog.Create(Array.Empty<Node>(), Array.Empty<Edge>(), Array.Empty<Project>());
        Assert.Equal(1, catalog.SchemaVersion);

        var state = State.Create(
            Array.Empty<ProjectTask>(), Array.Empty<Control>(), Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(), Array.Empty<Runbook>(), Array.Empty<AuditEvent>(), Array.Empty<UserStatement>());
        Assert.Equal(1, state.SchemaVersion);

        var workspace = Workspace.Create(Now, catalog, state);
        Assert.Equal("SOCPIT_WORKSPACE", Workspace.Format);
        Assert.Equal(1, workspace.SchemaVersion);
    }

    [Fact]
    public void Catalog_Create_NullCollections_Throw()
    {
        Assert.Throws<ArgumentException>(() => Catalog.Create(null!, Array.Empty<Edge>(), Array.Empty<Project>()));
        Assert.Throws<ArgumentException>(() => Catalog.Create(Array.Empty<Node>(), null!, Array.Empty<Project>()));
        Assert.Throws<ArgumentException>(() => Catalog.Create(Array.Empty<Node>(), Array.Empty<Edge>(), null!));
    }
}
