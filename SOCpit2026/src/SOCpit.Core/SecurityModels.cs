// SOCpit - Security Operations Center
// Security domain models for SOCpit project
// Based on MATRIX V3 with security extensions

using System;
using System.Collections.Generic;

namespace SOCpit.Core
{
    /// <summary>Threat severity levels.</summary>
    public enum ThreatSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>Threat status.</summary>
    public enum ThreatStatus
    {
        New,
        Investigating,
        Contained,
        Resolved,
        FalsePositive
    }

    /// <summary>Vulnerability severity levels.</summary>
    public enum VulnerabilitySeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>Vulnerability status.</summary>
    public enum VulnerabilityStatus
    {
        Open,
        InProgress,
        Fixed,
        NotApplicable,
        AcceptedRisk
    }

    /// <summary>Security incident severity.</summary>
    public enum IncidentSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>Security incident status.</summary>
    public enum IncidentStatus
    {
        New,
        Triage,
        Investigating,
        Contained,
        Resolved,
        Closed
    }

    /// <summary>Security control types.</summary>
    public enum SecurityControlType
    {
        Preventive,
        Detective,
        Corrective,
        Deterrent,
        Recovery,
        Compensating
    }

    /// <summary>Compliance status.</summary>
    public enum ComplianceStatus
    {
        Compliant,
        NonCompliant,
        Partial,
        NotApplicable,
        NotAssessed
    }

    /// <summary>A threat in the security landscape.</summary>
    public sealed class Threat
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required ThreatSeverity Severity { get; init; }
        public required ThreatStatus Status { get; init; }
        public required string TargetId { get; init; }
        public required DateTimeOffset DetectedAt { get; init; }
        public required string Source { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }
        public required string? MitigationSteps { get; init; }
        public required string? RelatedEvidenceIds { get; init; }

        public static Threat Create(
            string id, string name, string description, ThreatSeverity severity,
            ThreatStatus status, string targetId, DateTimeOffset detectedAt,
            string source, IReadOnlyList<string> tags, string? mitigationSteps = null,
            string? relatedEvidenceIds = null)
        {
            return new Threat
            {
                Id = ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Severity = severity,
                Status = status,
                TargetId = ValidateId(targetId),
                DetectedAt = detectedAt,
                Source = source ?? throw new ArgumentException("source"),
                Tags = tags ?? throw new ArgumentException("tags"),
                MitigationSteps = mitigationSteps,
                RelatedEvidenceIds = relatedEvidenceIds
            };
        }

        internal static string ValidateId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID must not be empty");
            if (id.Length > 80)
                throw new ArgumentException("ID exceeds 80 chars");
            if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^[a-zA-Z0-9][a-zA-Z0-9._-]*$"))
                throw new ArgumentException("ID has invalid characters");
            return id;
        }
    }

    /// <summary>A vulnerability in the system.</summary>
    public sealed class Vulnerability
    {
        public required string Id { get; init; }
        public required string CveId { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required VulnerabilitySeverity Severity { get; init; }
        public required VulnerabilityStatus Status { get; init; }
        public required string TargetId { get; init; }
        public required DateTimeOffset PublishedAt { get; init; }
        public required DateTimeOffset? LastUpdatedAt { get; init; }
        public required string? FixAvailable { get; init; }
        public required string? CvssScore { get; init; }
        public required IReadOnlyList<string> References { get; init; }

        public static Vulnerability Create(
            string id, string cveId, string name, string description,
            VulnerabilitySeverity severity, VulnerabilityStatus status,
            string targetId, DateTimeOffset publishedAt, string? fixAvailable = null,
            string? cvssScore = null, DateTimeOffset? lastUpdatedAt = null,
            IReadOnlyList<string>? references = null)
        {
            return new Vulnerability
            {
                Id = Threat.ValidateId(id),
                CveId = cveId ?? "N/A",
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Severity = severity,
                Status = status,
                TargetId = Threat.ValidateId(targetId),
                PublishedAt = publishedAt,
                LastUpdatedAt = lastUpdatedAt,
                FixAvailable = fixAvailable,
                CvssScore = cvssScore,
                References = references ?? new List<string>()
            };
        }
    }

    /// <summary>A security incident.</summary>
    public sealed class SecurityIncident
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required IncidentSeverity Severity { get; init; }
        public required IncidentStatus Status { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset? UpdatedAt { get; init; }
        public required DateTimeOffset? ResolvedAt { get; init; }
        public required string? AssignedTo { get; init; }
        public required IReadOnlyList<string> RelatedThreatIds { get; init; }
        public required IReadOnlyList<string> RelatedVulnerabilityIds { get; init; }
        public required string? ImpactAnalysis { get; init; }
        public required string? ResponseActions { get; init; }
        public required string? LessonsLearned { get; init; }

        public static SecurityIncident Create(
            string id, string title, string description, IncidentSeverity severity,
            IncidentStatus status, DateTimeOffset createdAt, string? assignedTo = null,
            IReadOnlyList<string>? relatedThreatIds = null,
            IReadOnlyList<string>? relatedVulnerabilityIds = null,
            string? impactAnalysis = null, string? responseActions = null,
            string? lessonsLearned = null, DateTimeOffset? updatedAt = null,
            DateTimeOffset? resolvedAt = null)
        {
            return new SecurityIncident
            {
                Id = Threat.ValidateId(id),
                Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("title") : title.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Severity = severity,
                Status = status,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ResolvedAt = resolvedAt,
                AssignedTo = assignedTo,
                RelatedThreatIds = relatedThreatIds ?? new List<string>(),
                RelatedVulnerabilityIds = relatedVulnerabilityIds ?? new List<string>(),
                ImpactAnalysis = impactAnalysis,
                ResponseActions = responseActions,
                LessonsLearned = lessonsLearned
            };
        }
    }

    /// <summary>A security control.</summary>
    public sealed class SecurityControl
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required SecurityControlType Type { get; init; }
        public required string Category { get; init; }
        public required bool Required { get; init; }
        public required string Scope { get; init; }
        public required string Implementation { get; init; }
        public required string? VerificationMethod { get; init; }
        public required int FrequencyDays { get; init; }
        public required DateTimeOffset LastVerifiedAt { get; init; }
        public required string? Owner { get; init; }

        public static SecurityControl Create(
            string id, string name, string description, SecurityControlType type,
            string category, bool required, string scope, string implementation,
            string? verificationMethod = null, int frequencyDays = 30,
            DateTimeOffset? lastVerifiedAt = null, string? owner = null)
        {
            return new SecurityControl
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Type = type,
                Category = category ?? throw new ArgumentException("category"),
                Required = required,
                Scope = scope ?? throw new ArgumentException("scope"),
                Implementation = implementation ?? throw new ArgumentException("implementation"),
                VerificationMethod = verificationMethod,
                FrequencyDays = frequencyDays < 1 ? 30 : frequencyDays,
                LastVerifiedAt = lastVerifiedAt ?? DateTimeOffset.UtcNow,
                Owner = owner
            };
        }
    }

    /// <summary>A compliance check.</summary>
    public sealed class ComplianceCheck
    {
        public required string Id { get; init; }
        public required string Standard { get; init; }
        public required string Requirement { get; init; }
        public required ComplianceStatus Status { get; init; }
        public required string TargetId { get; init; }
        public required DateTimeOffset CheckedAt { get; init; }
        public required DateTimeOffset? NextCheckAt { get; init; }
        public required string? EvidenceId { get; init; }
        public required string? Notes { get; init; }
        public required string? NonComplianceDetails { get; init; }

        public static ComplianceCheck Create(
            string id, string standard, string requirement, ComplianceStatus status,
            string targetId, DateTimeOffset checkedAt, string? evidenceId = null,
            string? notes = null, string? nonComplianceDetails = null,
            DateTimeOffset? nextCheckAt = null)
        {
            return new ComplianceCheck
            {
                Id = Threat.ValidateId(id),
                Standard = standard ?? throw new ArgumentException("standard"),
                Requirement = requirement ?? throw new ArgumentException("requirement"),
                Status = status,
                TargetId = Threat.ValidateId(targetId),
                CheckedAt = checkedAt,
                NextCheckAt = nextCheckAt,
                EvidenceId = evidenceId,
                Notes = notes,
                NonComplianceDetails = nonComplianceDetails
            };
        }
    }
}
