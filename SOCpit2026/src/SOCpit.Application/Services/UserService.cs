// SOCpit - Security Operations Center
// User service for application layer
// Based on MATRIX V3 with service pattern

using Microsoft.Extensions.Logging;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Services
{
    /// <summary>Service for managing users.</summary>
    public sealed class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task<User> CreateAsync(
            string username, string email, string fullName,
            UserRole role, UserStatus status, AuthenticationType authenticationType,
            IReadOnlyList<string> tags, string? passwordHash = null,
            string? department = null, string? phone = null,
            IReadOnlyList<string>? permissionIds = null,
            string? createdBy = null, CancellationToken cancellationToken = default)
        {
            var id = Guid.NewGuid().ToString();
            var user = User.Create(
                id: id,
                username: username,
                email: email,
                fullName: fullName,
                role: role,
                status: status,
                authenticationType: authenticationType,
                createdAt: DateTimeOffset.UtcNow,
                tags: tags,
                department: department,
                phone: phone,
                permissionIds: permissionIds);

            await _userRepository.AddAsync(user, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Create",
                entityType: "User",
                entityId: id,
                userId: createdBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Created user: {username} ({email})"), cancellationToken);

            _logger.LogInformation("Created user {UserId}: {Username} ({Email})", id, username, email);
            return user;
        }

        public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByUsernameAsync(username, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByEmailAsync(email, cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetAllAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        {
            return await _userRepository.GetByRoleAsync(role, cancellationToken);
        }

        public async Task<User> UpdateAsync(
            string id, string? username = null, string? email = null,
            string? fullName = null, UserRole? role = null,
            UserStatus? status = null, AuthenticationType? authenticationType = null,
            string? department = null, string? phone = null,
            IReadOnlyList<string>? tags = null, IReadOnlyList<string>? permissionIds = null,
            string? updatedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            var updated = User.Create(
                id: id,
                username: username ?? existing.Username,
                email: email ?? existing.Email,
                fullName: fullName ?? existing.FullName,
                role: role ?? existing.Role,
                status: status ?? existing.Status,
                authenticationType: authenticationType ?? existing.AuthenticationType,
                createdAt: existing.CreatedAt,
                tags: tags ?? existing.Tags,
                department: department ?? existing.Department,
                phone: phone ?? existing.Phone,
                permissionIds: permissionIds ?? existing.PermissionIds,
                lastLoginAt: existing.LastLoginAt);

            await _userRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Update",
                entityType: "User",
                entityId: id,
                userId: updatedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Updated user: {updated.Username}"), cancellationToken);

            _logger.LogInformation("Updated user {UserId}: {Username}", id, updated.Username);
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, string? deletedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                return false;

            var result = await _userRepository.DeleteAsync(id, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Delete",
                entityType: "User",
                entityId: id,
                userId: deletedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Deleted user: {existing.Username}"), cancellationToken);

            _logger.LogInformation("Deleted user {UserId}: {Username}", id, existing.Username);
            return result;
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _userRepository.CountAsync(cancellationToken);
        }

        public async Task<User> LockAsync(string id, DateTimeOffset? lockedUntil = null, string? lockedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            var updated = User.Create(
                id: id,
                username: existing.Username,
                email: existing.Email,
                fullName: existing.FullName,
                role: existing.Role,
                status: UserStatus.Locked,
                authenticationType: existing.AuthenticationType,
                createdAt: existing.CreatedAt,
                tags: existing.Tags,
                department: existing.Department,
                phone: existing.Phone,
                permissionIds: existing.PermissionIds,
                lastLoginAt: existing.LastLoginAt,
                lockedUntil: lockedUntil ?? DateTimeOffset.UtcNow.AddDays(30),
                failedLoginAttempts: existing.FailedLoginAttempts);

            await _userRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Lock",
                entityType: "User",
                entityId: id,
                userId: lockedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Locked user: {updated.Username}"), cancellationToken);

            _logger.LogInformation("Locked user {UserId}: {Username}", id, updated.Username);
            return updated;
        }

        public async Task<User> UnlockAsync(string id, string? unlockedBy = null, CancellationToken cancellationToken = default)
        {
            var existing = await _userRepository.GetByIdAsync(id, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            var updated = User.Create(
                id: id,
                username: existing.Username,
                email: existing.Email,
                fullName: existing.FullName,
                role: existing.Role,
                status: UserStatus.Active,
                authenticationType: existing.AuthenticationType,
                createdAt: existing.CreatedAt,
                tags: existing.Tags,
                department: existing.Department,
                phone: existing.Phone,
                permissionIds: existing.PermissionIds,
                lastLoginAt: existing.LastLoginAt,
                lockedUntil: null,
                failedLoginAttempts: 0);

            await _userRepository.UpdateAsync(updated, cancellationToken);

            await _auditLogRepository.AddAsync(AuditLog.Create(
                id: Guid.NewGuid().ToString(),
                action: "Unlock",
                entityType: "User",
                entityId: id,
                userId: unlockedBy ?? "system",
                timestamp: DateTimeOffset.UtcNow,
                details: $"Unlocked user: {updated.Username}"), cancellationToken);

            _logger.LogInformation("Unlocked user {UserId}: {Username}", id, updated.Username);
            return updated;
        }
    }
}
