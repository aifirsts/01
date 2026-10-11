// SOCpit - Security Operations Center
// Graph validation and conservative security policy (ADR-V2-SEC-001) for SOCpit
// Based on MATRIX V3 GraphValidator & SecurityPolicy

using System;
using System.Collections.Generic;
using System.Linq;

namespace SOCpit.Core
{
    /// <summary>
    /// ADR-V2-SEC-001: Conservative security policy.
    /// UNKNOWN, STALE, MISSING, empty set and user statements never produce PASS.
    /// </summary>
    public static class SecurityPolicy
    {
        public const int MaxJsonDepth = 32;
        public const long MaxInputBytes = 16 * 1024 * 1024;

        /// <summary>
        /// Validate supersedes graph on control evidence: no missing targets, no self-supersedes, no cycles.
        /// </summary>
        public static bool ValidateSupersedes(IReadOnlyList<ControlEvidence> evidenceList, out string? error)
        {
            error = null;
            if (evidenceList == null)
            {
                error = "Evidence list is null";
                return false;
            }

            var map = new Dictionary<string, ControlEvidence>();
            foreach (var ev in evidenceList)
            {
                if (ev == null)
                {
                    error = "Null evidence item";
                    return false;
                }
                if (!map.TryAdd(ev.Id, ev))
                {
                    error = $"Duplicate evidence ID: {ev.Id}";
                    return false;
                }
            }

            foreach (var ev in evidenceList)
            {
                foreach (var supId in ev.Supersedes)
                {
                    if (supId == ev.Id)
                    {
                        error = $"Evidence '{ev.Id}' cannot supersede itself";
                        return false;
                    }
                    if (!map.TryGetValue(supId, out var target))
                    {
                        error = $"Evidence '{ev.Id}' supersedes missing evidence '{supId}'";
                        return false;
                    }
                    if (target.ControlId != ev.ControlId)
                    {
                        error = $"Evidence '{ev.Id}' supersedes evidence '{supId}' from a different control";
                        return false;
                    }
                }
            }

            var visiting = new HashSet<string>();
            var visited = new HashSet<string>();
            foreach (var ev in evidenceList)
            {
                if (HasSupersedesCycle(ev.Id, map, visiting, visited))
                {
                    error = $"Supersedes cycle detected involving '{ev.Id}'";
                    return false;
                }
            }

            return true;
        }

        private static bool HasSupersedesCycle(
            string id,
            IReadOnlyDictionary<string, ControlEvidence> map,
            HashSet<string> visiting,
            HashSet<string> visited)
        {
            if (visited.Contains(id)) return false;
            if (!visiting.Add(id)) return true;

            if (map.TryGetValue(id, out var ev))
            {
                foreach (var sup in ev.Supersedes)
                {
                    if (map.ContainsKey(sup) && HasSupersedesCycle(sup, map, visiting, visited))
                        return true;
                }
            }

            visiting.Remove(id);
            visited.Add(id);
            return false;
        }

        /// <summary>
        /// Compute conservative security status for a node per ADR-V2-SEC-001.
        /// </summary>
        public static Security ComputeNodeSecurity(
            string nodeId,
            IReadOnlyList<Control> controls,
            IReadOnlyList<ControlEvidence> evidenceList,
            DateTimeOffset nowUtc)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || controls == null || evidenceList == null)
                return Security.Unknown;

            var nodeControls = controls.Where(c => c.TargetId == nodeId).ToList();
            if (nodeControls.Count == 0)
                return Security.Unknown;

            var applicableControls = nodeControls.Where(c => c.Applicability == ControlApplicability.Applicable).ToList();
            if (applicableControls.Count == 0)
            {
                return nodeControls.All(c => !string.IsNullOrWhiteSpace(c.Rationale))
                    ? Security.NotApplicable
                    : Security.Unknown;
            }

            var supersededIds = new HashSet<string>(
                evidenceList.Where(e => !e.Revoked).SelectMany(e => e.Supersedes));

            var hasUnknown = false;
            var hasBlocked = false;

            foreach (var control in applicableControls)
            {
                var validEvidence = evidenceList
                    .Where(e => e.ControlId == control.Id
                        && !e.Revoked
                        && !supersededIds.Contains(e.Id)
                        && e.ObservedAt <= nowUtc
                        && e.ValidUntil > nowUtc)
                    .OrderByDescending(e => e.ObservedAt)
                    .FirstOrDefault();

                if (validEvidence == null)
                {
                    if (control.Required)
                        hasUnknown = true;
                    continue;
                }

                switch (validEvidence.Result)
                {
                    case EvidenceResult.Fail:
                        return Security.Fail;
                    case EvidenceResult.Blocked:
                        hasBlocked = true;
                        break;
                    case EvidenceResult.Warning:
                        hasUnknown = true;
                        break;
                    case EvidenceResult.Pass:
                        break;
                }
            }

            if (hasBlocked) return Security.Blocked;
            if (hasUnknown) return Security.Unknown;
            return Security.Pass;
        }
    }

    /// <summary>
    /// Graph validation: global ID uniqueness, dangling edges, duplicate edges,
    /// task dependency DAG (no cycles, no duplicates, no self-deps).
    /// </summary>
    public static class GraphValidator
    {
        /// <summary>Validate catalog: node ID uniqueness, edge references, edge uniqueness.</summary>
        public static void ValidateCatalog(Catalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            var nodeIds = new HashSet<string>();
            foreach (var node in catalog.Nodes)
            {
                if (node == null)
                    throw new ArgumentException("Node in catalog is null");
                if (!nodeIds.Add(node.Id))
                    throw new ArgumentException($"Duplicate node ID: {node.Id}");
            }

            var edgeKeys = new HashSet<string>();
            foreach (var edge in catalog.Edges)
            {
                if (edge == null)
                    throw new ArgumentException("Edge in catalog is null");
                if (!nodeIds.Contains(edge.From))
                    throw new ArgumentException($"Dangling edge '{edge.Id}': from '{edge.From}' not found");
                if (!nodeIds.Contains(edge.To))
                    throw new ArgumentException($"Dangling edge '{edge.Id}': to '{edge.To}' not found");
                if (edge.From == edge.To)
                    throw new ArgumentException($"Self-referencing edge: {edge.Id}");

                var key = $"{edge.From}|{edge.To}|{edge.Kind}";
                if (!edgeKeys.Add(key))
                    throw new ArgumentException($"Duplicate edge relationship: {edge.Id} ({key})");
            }

            var projectNodeIds = new HashSet<string>(
                catalog.Nodes.Where(n => n.Type == NodeType.Project).Select(n => n.Id));
            foreach (var project in catalog.Projects)
            {
                if (project == null)
                    throw new ArgumentException("Project in catalog is null");
                if (!nodeIds.Contains(project.NodeId))
                    throw new ArgumentException($"Project '{project.Id}' references missing node '{project.NodeId}'");
                if (!projectNodeIds.Contains(project.NodeId))
                    throw new ArgumentException($"Project '{project.Id}' node '{project.NodeId}' is not type Project");
            }

            var projectIds = new HashSet<string>();
            foreach (var project in catalog.Projects)
            {
                if (!projectIds.Add(project.Id))
                    throw new ArgumentException($"Duplicate project ID: {project.Id}");
            }
        }

        /// <summary>Validate state: task dependencies DAG, control references, evidence references.</summary>
        public static void ValidateState(State state, Catalog catalog)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            var nodeIds = new HashSet<string>(catalog.Nodes.Select(n => n.Id));
            var projectIds = new HashSet<string>(catalog.Projects.Select(p => p.Id));
            var controlIds = new HashSet<string>();

            foreach (var control in state.Controls)
            {
                if (control == null)
                    throw new ArgumentException("Control in state is null");
                if (!nodeIds.Contains(control.TargetId))
                    throw new ArgumentException($"Control '{control.Id}' references missing node '{control.TargetId}'");
                if (!controlIds.Add(control.Id))
                    throw new ArgumentException($"Duplicate control ID: {control.Id}");
            }

            var evidenceIds = new HashSet<string>();
            foreach (var evidence in state.Evidence)
            {
                if (evidence == null)
                    throw new ArgumentException("Evidence in state is null");
                if (!controlIds.Contains(evidence.ControlId))
                    throw new ArgumentException($"Evidence '{evidence.Id}' references missing control '{evidence.ControlId}'");
                if (!evidenceIds.Add(evidence.Id))
                    throw new ArgumentException($"Duplicate evidence ID: {evidence.Id}");
            }

            // Validate supersedes
            if (!SecurityPolicy.ValidateSupersedes(state.Evidence, out var supersedesError))
                throw new ArgumentException(supersedesError ?? "Supersedes validation failed");

            var taskIds = new HashSet<string>();
            foreach (var task in state.Tasks)
            {
                if (task == null)
                    throw new ArgumentException("Task in state is null");
                if (!projectIds.Contains(task.ProjectId))
                    throw new ArgumentException($"Task '{task.Id}' references missing project '{task.ProjectId}'");
                if (!taskIds.Add(task.Id))
                    throw new ArgumentException($"Duplicate task ID: {task.Id}");

                var depSet = new HashSet<string>();
                foreach (var dep in task.Dependencies)
                {
                    if (!depSet.Add(dep))
                        throw new ArgumentException($"Duplicate dependency '{dep}' in task '{task.Id}'");
                    if (dep == task.Id)
                        throw new ArgumentException($"Self-dependency in task '{task.Id}'");
                }
            }

            var taskMap = state.Tasks.ToDictionary(t => t.Id, t => t.Dependencies);
            foreach (var taskId in taskIds)
            {
                ValidateTaskDfs(taskId, taskMap, new HashSet<string>(), new HashSet<string>(), out var cycleTask);
                if (cycleTask != null)
                    throw new ArgumentException($"Task dependency cycle detected involving '{cycleTask}'");
            }

            var findingIds = new HashSet<string>();
            foreach (var finding in state.Findings)
            {
                if (finding == null)
                    throw new ArgumentException("Finding in state is null");
                if (!nodeIds.Contains(finding.TargetId))
                    throw new ArgumentException($"Finding '{finding.Id}' references missing node '{finding.TargetId}'");
                if (!findingIds.Add(finding.Id))
                    throw new ArgumentException($"Duplicate finding ID: {finding.Id}");
            }

            var runbookIds = new HashSet<string>();
            foreach (var runbook in state.Runbooks)
            {
                if (runbook == null)
                    throw new ArgumentException("Runbook in state is null");
                if (!nodeIds.Contains(runbook.TargetId))
                    throw new ArgumentException($"Runbook '{runbook.Id}' references missing node '{runbook.TargetId}'");
                if (!runbookIds.Add(runbook.Id))
                    throw new ArgumentException($"Duplicate runbook ID: {runbook.Id}");
            }

            var statementIds = new HashSet<string>();
            foreach (var statement in state.UserStatements)
            {
                if (statement == null)
                    throw new ArgumentException("UserStatement in state is null");
                if (!nodeIds.Contains(statement.TargetId))
                    throw new ArgumentException($"UserStatement '{statement.Id}' references missing node '{statement.TargetId}'");
                if (!statementIds.Add(statement.Id))
                    throw new ArgumentException($"Duplicate user statement ID: {statement.Id}");
            }

            var auditIds = new HashSet<string>();
            foreach (var audit in state.AuditEvents)
            {
                if (audit == null)
                    throw new ArgumentException("AuditEvent in state is null");
                if (!auditIds.Add(audit.Id))
                    throw new ArgumentException($"Duplicate audit event ID: {audit.Id}");
            }
        }

        private static void ValidateTaskDfs(
            string taskId,
            IReadOnlyDictionary<string, IReadOnlyList<string>> taskMap,
            HashSet<string> visiting,
            HashSet<string> visited,
            out string? cycleTask)
        {
            cycleTask = null;
            if (visited.Contains(taskId)) return;
            if (!visiting.Add(taskId))
            {
                cycleTask = taskId;
                return;
            }

            if (taskMap.TryGetValue(taskId, out var deps))
            {
                foreach (var dep in deps)
                {
                    if (taskMap.ContainsKey(dep))
                        ValidateTaskDfs(dep, taskMap, visiting, visited, out cycleTask);
                    if (cycleTask != null) return;
                }
            }

            visiting.Remove(taskId);
            visited.Add(taskId);
        }

        /// <summary>Validate the complete workspace: catalog + state.</summary>
        public static void ValidateWorkspace(Workspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            ValidateCatalog(workspace.Catalog);
            ValidateState(workspace.State, workspace.Catalog);
        }
    }
}
