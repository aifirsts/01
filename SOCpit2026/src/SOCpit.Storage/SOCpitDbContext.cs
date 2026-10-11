// SOCpit - Security Operations Center
// Database context for SOCpit project
// Based on MATRIX V3 with Entity Framework Core

using SOCpit.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SOCpit.Storage
{
    /// <summary>Database context for SOCpit application.</summary>
    public sealed class SOCpitDbContext : DbContext
    {
        public SOCpitDbContext(DbContextOptions<SOCpitDbContext> options) : base(options)
        {
        }

        // Threat entities
        public DbSet<Threat> Threats => Set<Threat>();

        // Vulnerability entities
        public DbSet<Vulnerability> Vulnerabilities => Set<Vulnerability>();

        // Incident entities
        public DbSet<SecurityIncident> SecurityIncidents => Set<SecurityIncident>();

        // Evidence entities
        public DbSet<Evidence> Evidence => Set<Evidence>();

        // Infrastructure entities
        public DbSet<Host> Hosts => Set<Host>();
        public DbSet<Network> Networks => Set<Network>();
        public DbSet<Subnet> Subnets => Set<Subnet>();
        public DbSet<Vlan> Vlans => Set<Vlan>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<Application> Applications => Set<Application>();

        // User entities
        public DbSet<User> Users => Set<User>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Session> Sessions => Set<Session>();

        // Audit entities
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<UserAuditEntry> UserAuditEntries => Set<UserAuditEntry>();

        // Security entities
        public DbSet<SecurityControl> SecurityControls => Set<SecurityControl>();
        public DbSet<ComplianceCheck> ComplianceChecks => Set<ComplianceCheck>();

        // Evidence related entities
        public DbSet<LogEntry> LogEntries => Set<LogEntry>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<NetworkConnection> NetworkConnections => Set<NetworkConnection>();

        private static readonly ValueConverter<IReadOnlyList<string>, string> StringListConverter =
            new(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        private static readonly ValueConverter<IReadOnlyList<int>, string> IntListConverter =
            new(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>());

        private static readonly ValueConverter<IReadOnlyDictionary<string, string>?, string?> DictConverter =
            new(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v) ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all entities
            ConfigureThreat(modelBuilder);
            ConfigureVulnerability(modelBuilder);
            ConfigureSecurityIncident(modelBuilder);
            ConfigureEvidence(modelBuilder);
            ConfigureHost(modelBuilder);
            ConfigureNetwork(modelBuilder);
            ConfigureSubnet(modelBuilder);
            ConfigureVlan(modelBuilder);
            ConfigureService(modelBuilder);
            ConfigureApplication(modelBuilder);
            ConfigureUser(modelBuilder);
            ConfigurePermission(modelBuilder);
            ConfigureRole(modelBuilder);
            ConfigureGroup(modelBuilder);
            ConfigureSession(modelBuilder);
            ConfigureAuditLog(modelBuilder);
            ConfigureUserAuditEntry(modelBuilder);
            ConfigureSecurityControl(modelBuilder);
            ConfigureComplianceCheck(modelBuilder);
            ConfigureLogEntry(modelBuilder);
            ConfigureReport(modelBuilder);
            ConfigureNetworkConnection(modelBuilder);
        }

        private static void ConfigureThreat(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Threat>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id).ValueGeneratedNever();
                entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).IsRequired();
                entity.Property(t => t.Severity).IsRequired();
                entity.Property(t => t.Status).IsRequired();
                entity.Property(t => t.TargetId).IsRequired().HasMaxLength(80);
                entity.Property(t => t.DetectedAt).IsRequired();
                entity.Property(t => t.Source).IsRequired().HasMaxLength(100);
                entity.Property(t => t.Tags).HasConversion(StringListConverter).IsRequired();
                entity.Property(t => t.MitigationSteps);
                entity.Property(t => t.RelatedEvidenceIds);
            });
        }

        private static void ConfigureVulnerability(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vulnerability>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).ValueGeneratedNever();
                entity.Property(v => v.CveId).IsRequired().HasMaxLength(50);
                entity.Property(v => v.Name).IsRequired().HasMaxLength(200);
                entity.Property(v => v.Description).IsRequired();
                entity.Property(v => v.Severity).IsRequired();
                entity.Property(v => v.Status).IsRequired();
                entity.Property(v => v.TargetId).IsRequired().HasMaxLength(80);
                entity.Property(v => v.PublishedAt).IsRequired();
                entity.Property(v => v.LastUpdatedAt);
                entity.Property(v => v.FixAvailable);
                entity.Property(v => v.CvssScore);
                entity.Property(v => v.References).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureSecurityIncident(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SecurityIncident>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).ValueGeneratedNever();
                entity.Property(i => i.Title).IsRequired().HasMaxLength(200);
                entity.Property(i => i.Description).IsRequired();
                entity.Property(i => i.Severity).IsRequired();
                entity.Property(i => i.Status).IsRequired();
                entity.Property(i => i.CreatedAt).IsRequired();
                entity.Property(i => i.UpdatedAt);
                entity.Property(i => i.ResolvedAt);
                entity.Property(i => i.AssignedTo).HasMaxLength(80);
                entity.Property(i => i.RelatedThreatIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(i => i.RelatedVulnerabilityIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(i => i.ImpactAnalysis);
                entity.Property(i => i.ResponseActions);
                entity.Property(i => i.LessonsLearned);
            });
        }

        private static void ConfigureEvidence(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Evidence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Classification).IsRequired();
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CollectedAt).IsRequired();
                entity.Property(e => e.SizeBytes).IsRequired();
                entity.Property(e => e.Hash).IsRequired().HasMaxLength(64);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Tags).HasConversion(StringListConverter).IsRequired();
                entity.Property(e => e.RelatedIncidentIds);
                entity.Property(e => e.RelatedThreatIds);
                entity.Property(e => e.RelatedVulnerabilityIds);
                entity.Property(e => e.AnalysisNotes);
                entity.Property(e => e.CollectedBy).HasMaxLength(80);
            });
        }

        private static void ConfigureHost(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Host>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.Id).ValueGeneratedNever();
                entity.Property(h => h.Name).IsRequired().HasMaxLength(200);
                entity.Property(h => h.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(h => h.MacAddress).IsRequired().HasMaxLength(17);
                entity.Property(h => h.Type).IsRequired();
                entity.Property(h => h.Status).IsRequired();
                entity.Property(h => h.OperatingSystem).IsRequired();
                entity.Property(h => h.OsVersion).HasMaxLength(50);
                entity.Property(h => h.Hostname).HasMaxLength(255);
                entity.Property(h => h.Domain).HasMaxLength(255);
                entity.Property(h => h.Location).HasMaxLength(200);
                entity.Property(h => h.Tags).HasConversion(StringListConverter).IsRequired();
                entity.Property(h => h.Owner).HasMaxLength(80);
                entity.Property(h => h.LastSeenAt);
                entity.Property(h => h.CreatedAt).IsRequired();
            });
        }

        private static void ConfigureNetwork(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Network>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id).ValueGeneratedNever();
                entity.Property(n => n.Name).IsRequired().HasMaxLength(200);
                entity.Property(n => n.Cidr).IsRequired().HasMaxLength(18);
                entity.Property(n => n.Description).IsRequired();
                entity.Property(n => n.Type).IsRequired().HasMaxLength(50);
                entity.Property(n => n.VlanIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(n => n.SubnetIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(n => n.Tags).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureSubnet(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subnet>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedNever();
                entity.Property(s => s.NetworkId).IsRequired().HasMaxLength(80);
                entity.Property(s => s.Cidr).IsRequired().HasMaxLength(18);
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Description).IsRequired();
                entity.Property(s => s.HostIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(s => s.Tags).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureVlan(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vlan>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Id).ValueGeneratedNever();
                entity.Property(v => v.NetworkId).IsRequired().HasMaxLength(80);
                entity.Property(v => v.VlanId).IsRequired();
                entity.Property(v => v.Name).IsRequired().HasMaxLength(200);
                entity.Property(v => v.Description).IsRequired();
                entity.Property(v => v.Tags).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureService(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedNever();
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Description).IsRequired();
                entity.Property(s => s.Type).IsRequired().HasMaxLength(100);
                entity.Property(s => s.HostIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(s => s.Ports).HasConversion(IntListConverter).IsRequired();
                entity.Property(s => s.Status).IsRequired().HasMaxLength(50);
                entity.Property(s => s.Version).HasMaxLength(50);
                entity.Property(s => s.Owner).HasMaxLength(80);
                entity.Property(s => s.Tags).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureApplication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).ValueGeneratedNever();
                entity.Property(a => a.Name).IsRequired().HasMaxLength(200);
                entity.Property(a => a.Description).IsRequired();
                entity.Property(a => a.Version).IsRequired().HasMaxLength(50);
                entity.Property(a => a.Vendor).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Type).IsRequired().HasMaxLength(100);
                entity.Property(a => a.ServiceIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(a => a.HostIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(a => a.Owner).HasMaxLength(80);
                entity.Property(a => a.Tags).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedNever();
                entity.Property(u => u.Username).IsRequired().HasMaxLength(80);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
                entity.Property(u => u.Role).IsRequired();
                entity.Property(u => u.Status).IsRequired();
                entity.Property(u => u.AuthenticationType).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.LastLoginAt);
                entity.Property(u => u.LockedUntil);
                entity.Property(u => u.FailedLoginAttempts).IsRequired();
                entity.Property(u => u.Department).HasMaxLength(100);
                entity.Property(u => u.Phone).HasMaxLength(20);
                entity.Property(u => u.Tags).HasConversion(StringListConverter).IsRequired();
                entity.Property(u => u.PermissionIds).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigurePermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedNever();
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).IsRequired();
                entity.Property(p => p.Category).IsRequired().HasMaxLength(50);
                entity.Property(p => p.ResourceType).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Actions).HasConversion(StringListConverter).IsRequired();
                entity.Property(p => p.IsSystemPermission).IsRequired();
            });
        }

        private static void ConfigureRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).ValueGeneratedNever();
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Description).IsRequired();
                entity.Property(r => r.IsSystemRole).IsRequired();
                entity.Property(r => r.PermissionIds).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureGroup(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).ValueGeneratedNever();
                entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
                entity.Property(g => g.Description).IsRequired();
                entity.Property(g => g.UserIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(g => g.RoleIds).HasConversion(StringListConverter).IsRequired();
                entity.Property(g => g.PermissionIds).HasConversion(StringListConverter).IsRequired();
            });
        }

        private static void ConfigureSession(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedNever();
                entity.Property(s => s.UserId).IsRequired().HasMaxLength(80);
                entity.Property(s => s.Token).IsRequired().HasMaxLength(500);
                entity.Property(s => s.CreatedAt).IsRequired();
                entity.Property(s => s.ExpiresAt).IsRequired();
                entity.Property(s => s.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(s => s.UserAgent).IsRequired().HasMaxLength(500);
                entity.Property(s => s.IsActive).IsRequired();
                entity.Property(s => s.RefreshToken).HasMaxLength(500);
            });
        }

        private static void ConfigureAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Id).ValueGeneratedNever();
                entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
                entity.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(a => a.EntityId).IsRequired().HasMaxLength(80);
                entity.Property(a => a.UserId).IsRequired().HasMaxLength(80);
                entity.Property(a => a.Timestamp).IsRequired();
                entity.Property(a => a.Details);
                entity.Property(a => a.OldValue);
                entity.Property(a => a.NewValue);
                entity.Property(a => a.IpAddress).HasMaxLength(45);
                entity.Property(a => a.UserAgent).HasMaxLength(500);
                entity.Property(a => a.Success).IsRequired();
                entity.Property(a => a.ErrorMessage);
            });
        }

        private static void ConfigureUserAuditEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAuditEntry>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedNever();
                entity.Property(u => u.UserId).IsRequired().HasMaxLength(80);
                entity.Property(u => u.Action).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Timestamp).IsRequired();
                entity.Property(u => u.Details);
                entity.Property(u => u.IpAddress).HasMaxLength(45);
                entity.Property(u => u.UserAgent).HasMaxLength(500);
                entity.Property(u => u.Success).IsRequired();
            });
        }

        private static void ConfigureSecurityControl(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SecurityControl>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedNever();
                entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Description).IsRequired();
                entity.Property(s => s.Type).IsRequired();
                entity.Property(s => s.Category).IsRequired().HasMaxLength(100);
                entity.Property(s => s.Required).IsRequired();
                entity.Property(s => s.Scope).IsRequired().HasMaxLength(200);
                entity.Property(s => s.Implementation).IsRequired();
                entity.Property(s => s.VerificationMethod);
                entity.Property(s => s.FrequencyDays).IsRequired();
                entity.Property(s => s.LastVerifiedAt).IsRequired();
                entity.Property(s => s.Owner).HasMaxLength(80);
            });
        }

        private static void ConfigureComplianceCheck(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComplianceCheck>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).ValueGeneratedNever();
                entity.Property(c => c.Standard).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Requirement).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Status).IsRequired();
                entity.Property(c => c.TargetId).IsRequired().HasMaxLength(80);
                entity.Property(c => c.CheckedAt).IsRequired();
                entity.Property(c => c.NextCheckAt);
                entity.Property(c => c.EvidenceId).HasMaxLength(80);
                entity.Property(c => c.Notes);
                entity.Property(c => c.NonComplianceDetails);
            });
        }

        private static void ConfigureLogEntry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogEntry>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Id).ValueGeneratedNever();
                entity.Property(l => l.Source).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Message).IsRequired();
                entity.Property(l => l.Level).IsRequired().HasMaxLength(20);
                entity.Property(l => l.Timestamp).IsRequired();
                entity.Property(l => l.Host).HasMaxLength(255);
                entity.Property(l => l.Application).HasMaxLength(100);
                entity.Property(l => l.ProcessId).HasMaxLength(20);
                entity.Property(l => l.ThreadId).HasMaxLength(20);
                entity.Property(l => l.Exception);
                entity.Property(l => l.Properties).HasConversion(DictConverter);
            });
        }

        private static void ConfigureReport(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).ValueGeneratedNever();
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Type).IsRequired().HasMaxLength(100);
                entity.Property(r => r.Content).IsRequired();
                entity.Property(r => r.GeneratedAt).IsRequired();
                entity.Property(r => r.GeneratedBy).IsRequired().HasMaxLength(80);
                entity.Property(r => r.TemplateId).HasMaxLength(80);
                entity.Property(r => r.Parameters).HasConversion(StringListConverter).IsRequired();
                entity.Property(r => r.FilePath).IsRequired().HasMaxLength(500);
            });
        }

        private static void ConfigureNetworkConnection(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NetworkConnection>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id).ValueGeneratedNever();
                entity.Property(n => n.SourceIp).IsRequired().HasMaxLength(45);
                entity.Property(n => n.SourcePort).IsRequired();
                entity.Property(n => n.DestinationIp).IsRequired().HasMaxLength(45);
                entity.Property(n => n.DestinationPort).IsRequired();
                entity.Property(n => n.Protocol).IsRequired();
                entity.Property(n => n.EstablishedAt).IsRequired();
                entity.Property(n => n.TerminatedAt);
                entity.Property(n => n.BytesSent).IsRequired();
                entity.Property(n => n.BytesReceived).IsRequired();
                entity.Property(n => n.ProcessName).HasMaxLength(255);
                entity.Property(n => n.UserId).HasMaxLength(80);
            });
        }
    }
}
