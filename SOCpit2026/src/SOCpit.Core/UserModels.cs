// SOCpit - Security Operations Center
// User and identity domain models for SOCpit project
// Based on MATRIX V3 with user management

using System;
using System.Collections.Generic;

namespace SOCpit.Core
{
    /// <summary>User roles.</summary>
    public enum UserRole
    {
        Viewer,
        Analyst,
        Investigator,
        Administrator,
        SuperAdministrator
    }

    /// <summary>User status.</summary>
    public enum UserStatus
    {
        Active,
        Inactive,
        Locked,
        Disabled,
        Pending
    }

    /// <summary>Authentication types.</summary>
    public enum AuthenticationType
    {
        Local,
        LDAP,
        ActiveDirectory,
        OAuth,
        SAML,
        Certificate,
        MultiFactor
    }

    /// <summary>A user.</summary>
    public sealed class User
    {
        public required string Id { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required UserRole Role { get; init; }
        public required UserStatus Status { get; init; }
        public required AuthenticationType AuthenticationType { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset? LastLoginAt { get; init; }
        public required DateTimeOffset? LockedUntil { get; init; }
        public required int FailedLoginAttempts { get; init; }
        public required string? Department { get; init; }
        public required string? Phone { get; init; }
        public required IReadOnlyList<string> Tags { get; init; }
        public required IReadOnlyList<string> PermissionIds { get; init; }

        public static User Create(
            string id, string username, string email, string fullName,
            UserRole role, UserStatus status, AuthenticationType authenticationType,
            DateTimeOffset createdAt, IReadOnlyList<string> tags,
            IReadOnlyList<string>? permissionIds = null,
            string? department = null, string? phone = null,
            DateTimeOffset? lastLoginAt = null, DateTimeOffset? lockedUntil = null,
            int failedLoginAttempts = 0)
        {
            return new User
            {
                Id = Threat.ValidateId(id),
                Username = string.IsNullOrWhiteSpace(username) ? throw new ArgumentException("username") : username.Trim(),
                Email = email ?? throw new ArgumentException("email"),
                FullName = string.IsNullOrWhiteSpace(fullName) ? throw new ArgumentException("fullName") : fullName.Trim(),
                Role = role,
                Status = status,
                AuthenticationType = authenticationType,
                CreatedAt = createdAt,
                LastLoginAt = lastLoginAt,
                LockedUntil = lockedUntil,
                FailedLoginAttempts = failedLoginAttempts >= 0 ? failedLoginAttempts : 0,
                Department = department,
                Phone = phone,
                Tags = tags ?? throw new ArgumentException("tags"),
                PermissionIds = permissionIds ?? new List<string>()
            };
        }
    }

    /// <summary>A permission.</summary>
    public sealed class Permission
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Category { get; init; }
        public required string ResourceType { get; init; }
        public required IReadOnlyList<string> Actions { get; init; }
        public required bool IsSystemPermission { get; init; }

        public static Permission Create(
            string id, string name, string description, string category,
            string resourceType, IReadOnlyList<string> actions, bool isSystemPermission = false)
        {
            return new Permission
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                Category = category ?? throw new ArgumentException("category"),
                ResourceType = resourceType ?? throw new ArgumentException("resourceType"),
                Actions = actions ?? throw new ArgumentException("actions"),
                IsSystemPermission = isSystemPermission
            };
        }
    }

    /// <summary>A role.</summary>
    public sealed class Role
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required bool IsSystemRole { get; init; }
        public required IReadOnlyList<string> PermissionIds { get; init; }

        public static Role Create(
            string id, string name, string description,
            IReadOnlyList<string> permissionIds, bool isSystemRole = false)
        {
            return new Role
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                IsSystemRole = isSystemRole,
                PermissionIds = permissionIds ?? throw new ArgumentException("permissionIds")
            };
        }
    }

    /// <summary>A group.</summary>
    public sealed class Group
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required IReadOnlyList<string> UserIds { get; init; }
        public required IReadOnlyList<string> RoleIds { get; init; }
        public required IReadOnlyList<string> PermissionIds { get; init; }

        public static Group Create(
            string id, string name, string description,
            IReadOnlyList<string> userIds, IReadOnlyList<string> roleIds,
            IReadOnlyList<string> permissionIds)
        {
            return new Group
            {
                Id = Threat.ValidateId(id),
                Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("name") : name.Trim(),
                Description = description ?? throw new ArgumentException("description"),
                UserIds = userIds ?? throw new ArgumentException("userIds"),
                RoleIds = roleIds ?? throw new ArgumentException("roleIds"),
                PermissionIds = permissionIds ?? throw new ArgumentException("permissionIds")
            };
        }
    }

    /// <summary>A session.</summary>
    public sealed class Session
    {
        public required string Id { get; init; }
        public required string UserId { get; init; }
        public required string Token { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset ExpiresAt { get; init; }
        public required string IpAddress { get; init; }
        public required string UserAgent { get; init; }
        public required bool IsActive { get; init; }
        public required string? RefreshToken { get; init; }

        public static Session Create(
            string id, string userId, string token, DateTimeOffset createdAt,
            DateTimeOffset expiresAt, string ipAddress, string userAgent,
            bool isActive = true, string? refreshToken = null)
        {
            return new Session
            {
                Id = Threat.ValidateId(id),
                UserId = Threat.ValidateId(userId),
                Token = token ?? throw new ArgumentException("token"),
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
                IpAddress = ipAddress ?? throw new ArgumentException("ipAddress"),
                UserAgent = userAgent ?? throw new ArgumentException("userAgent"),
                IsActive = isActive,
                RefreshToken = refreshToken
            };
        }
    }

    /// <summary>An audit entry for user actions.</summary>
    public sealed class UserAuditEntry
    {
        public required string Id { get; init; }
        public required string UserId { get; init; }
        public required string Action { get; init; }
        public required DateTimeOffset Timestamp { get; init; }
        public required string? Details { get; init; }
        public required string? IpAddress { get; init; }
        public required string? UserAgent { get; init; }
        public required bool Success { get; init; }

        public static UserAuditEntry Create(
            string id, string userId, string action, DateTimeOffset timestamp,
            string? details = null, string? ipAddress = null, string? userAgent = null,
            bool success = true)
        {
            return new UserAuditEntry
            {
                Id = Threat.ValidateId(id),
                UserId = Threat.ValidateId(userId),
                Action = action ?? throw new ArgumentException("action"),
                Timestamp = timestamp,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Success = success
            };
        }
    }
}
