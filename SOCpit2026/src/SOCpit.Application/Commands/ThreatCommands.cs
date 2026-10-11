// SOCpit - Security Operations Center
// Threat commands for CQRS pattern
// Based on MATRIX V3 with command pattern

using Microsoft.Extensions.Logging;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Commands
{
    /// <summary>Command to create a new threat.</summary>
    public sealed record CreateThreatCommand(
        string Name,
        string Description,
        ThreatSeverity Severity,
        ThreatStatus Status,
        string TargetId,
        string Source,
        IReadOnlyList<string> Tags,
        string? MitigationSteps = null,
        string? RelatedEvidenceIds = null) : ICommand<Threat>;

    /// <summary>Command to update a threat.</summary>
    public sealed record UpdateThreatCommand(
        string Id,
        string? Name = null,
        string? Description = null,
        ThreatSeverity? Severity = null,
        ThreatStatus? Status = null,
        string? TargetId = null,
        string? Source = null,
        IReadOnlyList<string>? Tags = null,
        string? MitigationSteps = null,
        string? RelatedEvidenceIds = null) : ICommand<Threat>;

    /// <summary>Command to delete a threat.</summary>
    public sealed record DeleteThreatCommand(string Id) : ICommand<bool>;

    /// <summary>Command handler for threat commands.</summary>
    public sealed class ThreatCommandHandler :
        ICommandHandler<CreateThreatCommand, Threat>,
        ICommandHandler<UpdateThreatCommand, Threat>,
        ICommandHandler<DeleteThreatCommand, bool>
    {
        private readonly IThreatRepository _threatRepository;
        private readonly ILogger<ThreatCommandHandler> _logger;

        public ThreatCommandHandler(
            IThreatRepository threatRepository,
            ILogger<ThreatCommandHandler> logger)
        {
            _threatRepository = threatRepository;
            _logger = logger;
        }

        public async Task<Threat> Handle(
            CreateThreatCommand command, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid().ToString();
            var threat = Threat.Create(
                id: id,
                name: command.Name,
                description: command.Description,
                severity: command.Severity,
                status: command.Status,
                targetId: command.TargetId,
                detectedAt: DateTimeOffset.UtcNow,
                source: command.Source,
                tags: command.Tags,
                mitigationSteps: command.MitigationSteps,
                relatedEvidenceIds: command.RelatedEvidenceIds);

            await _threatRepository.AddAsync(threat, cancellationToken);
            _logger.LogInformation("Created threat {ThreatId}", id);
            return threat;
        }

        public async Task<Threat> Handle(
            UpdateThreatCommand command, CancellationToken cancellationToken)
        {
            var existing = await _threatRepository.GetByIdAsync(command.Id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"Threat with ID {command.Id} not found");

            var updated = Threat.Create(
                id: command.Id,
                name: command.Name ?? existing.Name,
                description: command.Description ?? existing.Description,
                severity: command.Severity ?? existing.Severity,
                status: command.Status ?? existing.Status,
                targetId: command.TargetId ?? existing.TargetId,
                detectedAt: existing.DetectedAt,
                source: command.Source ?? existing.Source,
                tags: command.Tags ?? existing.Tags,
                mitigationSteps: command.MitigationSteps ?? existing.MitigationSteps,
                relatedEvidenceIds: command.RelatedEvidenceIds ?? existing.RelatedEvidenceIds);

            await _threatRepository.UpdateAsync(updated, cancellationToken);
            _logger.LogInformation("Updated threat {ThreatId}", command.Id);
            return updated;
        }

        public async Task<bool> Handle(
            DeleteThreatCommand command, CancellationToken cancellationToken)
        {
            var deleted = await _threatRepository.DeleteAsync(command.Id, cancellationToken);
            if (deleted)
                _logger.LogInformation("Deleted threat {ThreatId}", command.Id);
            return deleted;
        }
    }
}
