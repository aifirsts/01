// SOCpit - Security Operations Center
// Evidence domain models for SOCpit project
// Based on MATRIX V3 with evidence management

using System;
using System.Collections.Generic;

namespace SOCpit.Core
{
    /// <summary>Evidence types.</summary>
    public enum EvidenceType
    {
        Log,
        Screenshot,
        NetworkCapture,
        File,
        MemoryDump,
        Configuration,
        AuditLog,
        Alert,
        Report,
        Other
    }

    /// <summary>Evidence status.</summary>
    public enum EvidenceStatus
    {
        New,
        Reviewed,
        Analyzed,
        Archived,
        Deleted
    }

    /// <summary>Evidence classification.</summary>
    public enum EvidenceClassification
    {
        Public,
        Internal,
        Confidential,
        Restricted
    }

    /// <summary>A piece of evidence.</summary>
    public sealed class Evidence
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required EvidenceType Type { get; init; }
        public required string Description { get; init; }
        public required EvidenceStatus Status { get; init; }
        public required EvidenceClassification Classification { get; init; }
        public required string Source { get; init; }
        public required DateTimeOffset CollectedAt { get; init; }
        public required long SizeBytes { get; init; }
        public required string Hash { get; init; }
        public required string FilePath { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }
        public required string? RelatedIncidentIds { get; init; }
        public required string? RelatedThreatIds { get; init; }
        public required string? RelatedVulnerabilityIds { get; init; }
        public required string? AnalysisNotes { get; init; }
        public required string? CollectedBy { get; init; }

        public static Evidence Create(
            string id, string name, EvidenceType type, string description,
            EvidenceStatus status, EvidenceClassification classification,
            string source, DateTimeOffset collectedAt, long sizeBytes,
            string hash, string filePath, IReadOnlyList<string> tags,
            string? relatedIncidentIds = null, string? relatedThreatIds = null,
            string? relatedVulnerabilityIds = null, string? analysisNotes = null,
            string? collectedBy = null)
        {
            return new Evidence
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Type = type,
                Description = description ?? throw new ArgumentException("description"),
                Status = status,
                Classification = classification,
                Source = source ?? throw new ArgumentException("source"),
                CollectedAt = collectedAt,
                SizeBytes = sizeBytes >= 0 ? sizeBytes : throw new ArgumentException("sizeBytes"),
                Hash = hash ?? throw new ArgumentException("hash"),
                FilePath = filePath ?? throw new ArgumentException("filePath"),
                Tags = tags ?? throw new ArgumentException("tags"),
                RelatedIncidentIds = relatedIncidentIds,
                RelatedThreatIds = relatedThreatIds,
                RelatedVulnerabilityIds = relatedVulnerabilityIds,
                AnalysisNotes = analysisNotes,
                CollectedBy = collectedBy
            };
        }
    }

    /// <summary>An audit log entry.</summary>
    public sealed class AuditLog
    {
        public required string Id { get; init; }
        public required string Action { get; init; }
        public required string EntityType { get; init; }
        public required string EntityId { get; init; }
        public required string UserId { get; init; }
        public required DateTimeOffset Timestamp { get; init; }
        public required string? Details { get; init; }
        public required string? OldValue { get; init; }
        public required string? NewValue { get; init; }
        public required string? IpAddress { get; init; }
        public required string? UserAgent { get; init; }
        public required bool Success { get; init; }
        public required string? ErrorMessage { get; init; }

        public static AuditLog Create(
            string id, string action, string entityType, string entityId,
            string userId, DateTimeOffset timestamp, string? details = null,
            string? oldValue = null, string? newValue = null,
            string? ipAddress = null, string? userAgent = null,
            bool success = true, string? errorMessage = null)
        {
            return new AuditLog
            {
                Id = Threat.ValidateId(id),
                Action = action ?? throw new ArgumentException("action"),
                EntityType = entityType ?? throw new ArgumentException("entityType"),
                EntityId = entityId ?? throw new ArgumentException("entityId"),
                UserId = userId ?? throw new ArgumentException("userId"),
                Timestamp = timestamp,
                Details = details,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = success,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>A log entry.</summary>
    public sealed class LogEntry
    {
        public required string Id { get; init; }
        public required string Source { get; init; }
        public required string Message { get; init; }
        public required string Level { get; init; }
        public required DateTimeOffset Timestamp { get; init; }
        public required string? Host { get; init; }
        public required string? Application { get; init; }
        public required string? ProcessId { get; init; }
        public required string? ThreadId { get; init; }
        public required string? Exception { get; init; }
        public required IReadOnlyDictionary<string, string>? Properties { get; init; }

        public static LogEntry Create(
            string id, string source, string message, string level,
            DateTimeOffset timestamp, string? host = null, string? application = null,
            string? processId = null, string? threadId = null, string? exception = null,
            IReadOnlyDictionary<string, string>? properties = null)
        {
            return new LogEntry
            {
                Id = Threat.ValidateId(id),
                Source = source ?? throw new ArgumentException("source"),
                Message = message ?? throw new ArgumentException("message"),
                Level = level ?? throw new ArgumentException("level"),
                Timestamp = timestamp,
                Host = host,
                Application = application,
                ProcessId = processId,
                ThreadId = threadId,
                Exception = exception,
                Properties = properties
            };
        }
    }

    /// <summary>A report.</summary>
    public sealed class Report
    {
        public required string Id { get; init; }
        public required string Title { get; init; }
        public required string Type { get; init; }
        public required string Content { get; init; }
        public required DateTimeOffset GeneratedAt { get; init; }
        public required string GeneratedBy { get; init; }
        public required string? TemplateId { get; init; }
        public required IReadOnlyList<string> Parameters { get; init; }
        public required string FilePath { get; init; }

        public static Report Create(
            string id, string title, string type, string content,
            DateTimeOffset generatedAt, string generatedBy, string filePath,
            string? templateId = null, IReadOnlyList<string>? parameters = null)
        {
            return new Report
            {
                Id = Threat.ValidateId(id),
                Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("title") : title.Trim(),
                Type = type ?? throw new ArgumentException("type"),
                Content = content ?? throw new ArgumentException("content"),
                GeneratedAt = generatedAt,
                GeneratedBy = generatedBy ?? throw new ArgumentException("generatedBy"),
                TemplateId = templateId,
                Parameters = parameters ?? new List<string>(),
                FilePath = filePath ?? throw new ArgumentException("filePath")
            };
        }
    }
}
