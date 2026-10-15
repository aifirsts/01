using System;
using System.Collections.Generic;
using SOCpit.Core;
using SOCpit.TestSupport;
using Xunit;

namespace SOCpit.Core.Tests;

/// <summary>
/// Graph integrity rules: unique IDs, no dangling references, no cycles,
/// valid task dependency DAGs.
/// </summary>
public class GraphValidatorTests
{
    private static readonly DateTimeOffset Now = TestData.Now;

    private static Node NewNode(string id, NodeType type = NodeType.Device)
        => Node.Create(id, type, "node-" + id, "NEO", "trust-zone", Sensitivity.Internal, Availability.Up);

    private static Project NewProject(string id, string nodeId)
        => Project.Create(id, nodeId, "goal", null, "stage", "next-action", new List<string>());

    private static ProjectTask NewTask(string id, string projectId, params string[] dependencies)
        => ProjectTask.Create(id, projectId, "task-" + id, ProjectTaskStatus.Todo, TaskPriority.Medium, dependencies);

    private static Finding NewFinding(string id, string targetId)
        => Finding.Create(id, targetId, FindingSeverity.Low, "impact", "remediation", FindingStatus.Open);

    private static Runbook NewRunbook(string id, string targetId)
        => Runbook.Create(id, targetId, "runbook-" + id, "preconditions", new List<string> { "step" }, "low", "verification");

    private static AuditEvent NewAuditEvent(string id)
        => AuditEvent.Create(id, Now, "NEO", "validate", "node-a", null, after: null);

    private static UserStatement NewStatement(string id, string targetId)
        => UserStatement.Create(id, targetId, "statement", Now, "NEO");

    private static Catalog ValidCatalog()
        => Catalog.Create(
            new[] { NewNode("node-a"), NewNode("project-node", NodeType.Project) },
            new[] { Edge.Create("edge-1", "node-a", "project-node", EdgeKind.DependsOn, Criticality.Medium) },
            new[] { NewProject("project-1", "project-node") });

    private static State ValidState()
        => State.Create(
            new[] { NewTask("task-1", "project-1") },
            new[] { TestData.NewControl(id: "ctrl-1", targetId: "node-a") },
            new[] { TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1") },
            new[] { NewFinding("finding-1", "node-a") },
            new[] { NewRunbook("runbook-1", "node-a") },
            new[] { NewAuditEvent("audit-1") },
            new[] { NewStatement("stmt-1", "node-a") });

    [Fact]
    public void ValidateCatalog_ValidCatalog_DoesNotThrow()
    {
        GraphValidator.ValidateCatalog(ValidCatalog());
    }

    [Fact]
    public void ValidateCatalog_NullCatalog_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GraphValidator.ValidateCatalog(null!));
    }

    [Fact]
    public void ValidateCatalog_DuplicateNodeId_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a"), NewNode("node-a") },
            Array.Empty<Edge>(),
            Array.Empty<Project>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("Duplicate node ID", ex.Message);
    }

    [Fact]
    public void ValidateCatalog_NullNode_Throws()
    {
        var catalog = Catalog.Create(new Node[] { null! }, Array.Empty<Edge>(), Array.Empty<Project>());
        Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
    }

    [Fact]
    public void ValidateCatalog_DanglingEdge_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a") },
            new[] { Edge.Create("edge-1", "node-a", "node-missing", EdgeKind.DependsOn, Criticality.Low) },
            Array.Empty<Project>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("Dangling edge", ex.Message);
    }

    [Fact]
    public void ValidateCatalog_SelfReferencingEdge_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a") },
            new[] { Edge.Create("edge-1", "node-a", "node-a", EdgeKind.ConnectsTo, Criticality.Low) },
            Array.Empty<Project>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("Self-referencing edge", ex.Message);
    }

    [Fact]
    public void ValidateCatalog_DuplicateEdgeRelationship_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a"), NewNode("node-b") },
            new[]
            {
                Edge.Create("edge-1", "node-a", "node-b", EdgeKind.ConnectsTo, Criticality.Low),
                Edge.Create("edge-2", "node-a", "node-b", EdgeKind.ConnectsTo, Criticality.High)
            },
            Array.Empty<Project>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("Duplicate edge relationship", ex.Message);
    }

    [Fact]
    public void ValidateCatalog_ProjectPointingToMissingNode_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a") },
            Array.Empty<Edge>(),
            new[] { NewProject("project-1", "node-missing") });

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("references missing node", ex.Message);
    }

    [Fact]
    public void ValidateCatalog_ProjectNodeOfWrongType_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a", NodeType.Device) },
            Array.Empty<Edge>(),
            new[] { NewProject("project-1", "node-a") });

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("is not type Project", ex.Message);
    }

    [Fact]
    public void ValidateCatalog_DuplicateProjectId_Throws()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("project-node", NodeType.Project) },
            Array.Empty<Edge>(),
            new[] { NewProject("project-1", "project-node"), NewProject("project-1", "project-node") });

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateCatalog(catalog));
        Assert.Contains("Duplicate project ID", ex.Message);
    }

    [Fact]
    public void ValidateState_ValidState_DoesNotThrow()
    {
        GraphValidator.ValidateState(ValidState(), ValidCatalog());
    }

    [Fact]
    public void ValidateState_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => GraphValidator.ValidateState(null!, ValidCatalog()));
        Assert.Throws<ArgumentNullException>(() => GraphValidator.ValidateState(ValidState(), null!));
    }

    [Fact]
    public void ValidateState_ControlReferencingMissingNode_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            new[] { TestData.NewControl(id: "ctrl-1", targetId: "node-missing") },
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("references missing node", ex.Message);
    }

    [Fact]
    public void ValidateState_DuplicateControlId_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            new[]
            {
                TestData.NewControl(id: "ctrl-dup", targetId: "node-a"),
                TestData.NewControl(id: "ctrl-dup", targetId: "node-a")
            },
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate control ID", ex.Message);
    }

    [Fact]
    public void ValidateState_EvidenceReferencingMissingControl_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            new[] { TestData.NewControl(id: "ctrl-1", targetId: "node-a") },
            new[] { TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-missing") },
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("references missing control", ex.Message);
    }

    [Fact]
    public void ValidateState_DuplicateEvidenceId_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            new[] { TestData.NewControl(id: "ctrl-1", targetId: "node-a") },
            new[]
            {
                TestData.NewControlEvidence(id: "cev-dup", controlId: "ctrl-1"),
                TestData.NewControlEvidence(id: "cev-dup", controlId: "ctrl-1")
            },
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate evidence ID", ex.Message);
    }

    [Fact]
    public void ValidateState_InvalidSupersedesChain_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            new[] { TestData.NewControl(id: "ctrl-1", targetId: "node-a") },
            new[] { TestData.NewControlEvidence(id: "cev-1", controlId: "ctrl-1", supersedes: new List<string> { "cev-ghost" }) },
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
    }

    [Fact]
    public void ValidateState_TaskReferencingMissingProject_Throws()
    {
        var state = State.Create(
            new[] { NewTask("task-1", "project-missing") },
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("references missing project", ex.Message);
    }

    [Fact]
    public void ValidateState_SelfDependency_Throws()
    {
        var state = State.Create(
            new[] { NewTask("task-1", "project-1", "task-1") },
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Self-dependency", ex.Message);
    }

    [Fact]
    public void ValidateState_DuplicateDependency_Throws()
    {
        var state = State.Create(
            new[]
            {
                NewTask("task-1", "project-1"),
                NewTask("task-2", "project-1", "task-1", "task-1")
            },
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate dependency", ex.Message);
    }

    [Fact]
    public void ValidateState_TaskDependencyCycle_Throws()
    {
        var state = State.Create(
            new[]
            {
                NewTask("task-1", "project-1", "task-2"),
                NewTask("task-2", "project-1", "task-1")
            },
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("cycle", ex.Message);
    }

    [Fact]
    public void ValidateState_DiamondDependencyGraph_IsAccepted()
    {
        var state = State.Create(
            new[]
            {
                NewTask("task-1", "project-1"),
                NewTask("task-2", "project-1", "task-1"),
                NewTask("task-3", "project-1", "task-1"),
                NewTask("task-4", "project-1", "task-2", "task-3")
            },
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        GraphValidator.ValidateState(state, ValidCatalog());
    }

    [Fact]
    public void ValidateState_DuplicateTaskId_Throws()
    {
        var state = State.Create(
            new[] { NewTask("task-dup", "project-1"), NewTask("task-dup", "project-1") },
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate task ID", ex.Message);
    }

    [Fact]
    public void ValidateState_DuplicateFindingId_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            new[] { NewFinding("finding-dup", "node-a"), NewFinding("finding-dup", "node-a") },
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate finding ID", ex.Message);
    }

    [Fact]
    public void ValidateState_FindingReferencingMissingNode_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            new[] { NewFinding("finding-1", "node-missing") },
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
    }

    [Fact]
    public void ValidateState_DuplicateRunbookId_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            new[] { NewRunbook("runbook-dup", "node-a"), NewRunbook("runbook-dup", "node-a") },
            Array.Empty<AuditEvent>(),
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate runbook ID", ex.Message);
    }

    [Fact]
    public void ValidateState_DuplicateUserStatementId_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            Array.Empty<AuditEvent>(),
            new[] { NewStatement("stmt-dup", "node-a"), NewStatement("stmt-dup", "node-a") });

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate user statement ID", ex.Message);
    }

    [Fact]
    public void ValidateState_DuplicateAuditEventId_Throws()
    {
        var state = State.Create(
            Array.Empty<ProjectTask>(),
            Array.Empty<Control>(),
            Array.Empty<ControlEvidence>(),
            Array.Empty<Finding>(),
            Array.Empty<Runbook>(),
            new[] { NewAuditEvent("audit-dup"), NewAuditEvent("audit-dup") },
            Array.Empty<UserStatement>());

        var ex = Assert.Throws<ArgumentException>(() => GraphValidator.ValidateState(state, ValidCatalog()));
        Assert.Contains("Duplicate audit event ID", ex.Message);
    }

    [Fact]
    public void ValidateWorkspace_ValidWorkspace_DoesNotThrow()
    {
        var workspace = Workspace.Create(Now, ValidCatalog(), ValidState());
        GraphValidator.ValidateWorkspace(workspace);
    }

    [Fact]
    public void ValidateWorkspace_NullWorkspace_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GraphValidator.ValidateWorkspace(null!));
    }

    [Fact]
    public void ValidateWorkspace_InvalidCatalog_PropagatesException()
    {
        var catalog = Catalog.Create(
            new[] { NewNode("node-a"), NewNode("node-a") },
            Array.Empty<Edge>(),
            Array.Empty<Project>());

        var workspace = Workspace.Create(Now, catalog, ValidState());
        Assert.Throws<ArgumentException>(() => GraphValidator.ValidateWorkspace(workspace));
    }

    [Fact]
    public void Workspace_ExposesFormatConstantAndSchemaVersion()
    {
        var workspace = Workspace.Create(Now, ValidCatalog(), ValidState());
        Assert.Equal("SOCPIT_WORKSPACE", Workspace.Format);
        Assert.Equal(1, workspace.SchemaVersion);
        Assert.Equal(1, workspace.Catalog.SchemaVersion);
        Assert.Equal(1, workspace.State.SchemaVersion);
        Assert.Equal(Now, workspace.ExportedAt);
    }
}
