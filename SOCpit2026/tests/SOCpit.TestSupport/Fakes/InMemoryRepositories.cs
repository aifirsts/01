// SOCpit - Security Operations Center
// In-memory repository fakes for fast application-layer unit tests.
// These fakes implement the very same repository contracts as the EF Core storage layer.

using SOCpit.Application.Repositories;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.TestSupport.Fakes
{
    /// <summary>Common behaviour for in-memory repository fakes.</summary>
    public abstract class InMemoryRepositoryBase<T> where T : class
    {
        private readonly Func<T, string> _idSelector;

        protected InMemoryRepositoryBase(Func<T, string> idSelector)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        /// <summary>Raw store, exposed for assertions.</summary>
        public Dictionary<string, T> Store { get; } = new(StringComparer.Ordinal);

        protected string IdOf(T entity) => _idSelector(entity);

        protected T Add(T entity)
        {
            Store[_idSelector(entity)] = entity;
            return entity;
        }

        protected T? Get(string id)
        {
            return Store.TryGetValue(id, out var entity) ? entity : null;
        }

        protected IReadOnlyList<T> GetAll()
        {
            return Store.Values.ToList();
        }

        protected T Update(T entity)
        {
            Store[_idSelector(entity)] = entity;
            return entity;
        }

        protected bool Delete(string id) => Store.Remove(id);

        protected int Count() => Store.Count;

        protected void Seed(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Store[_idSelector(entity)] = entity;
            }
        }
    }

    /// <summary>In-memory <see cref="IThreatRepository"/>.</summary>
    public sealed class InMemoryThreatRepository : InMemoryRepositoryBase<Threat>, IThreatRepository
    {
        public InMemoryThreatRepository() : base(t => t.Id) { }

        public void Seed(params Threat[] threats) => Seed((IEnumerable<Threat>)threats);

        public Task<Threat> AddAsync(Threat threat, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(threat));

        public Task<Threat?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<IReadOnlyList<Threat>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<IReadOnlyList<Threat>> GetBySeverityAsync(ThreatSeverity severity, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Threat>>(GetAll().Where(t => t.Severity == severity).ToList());

        public Task<IReadOnlyList<Threat>> GetByStatusAsync(ThreatStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Threat>>(GetAll().Where(t => t.Status == status).ToList());

        public Task<IReadOnlyList<Threat>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Threat>>(GetAll().Where(t => t.TargetId == targetId).ToList());

        public Task<Threat> UpdateAsync(Threat threat, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(threat));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="IVulnerabilityRepository"/>.</summary>
    public sealed class InMemoryVulnerabilityRepository : InMemoryRepositoryBase<Vulnerability>, IVulnerabilityRepository
    {
        public InMemoryVulnerabilityRepository() : base(v => v.Id) { }

        public void Seed(params Vulnerability[] vulnerabilities) => Seed((IEnumerable<Vulnerability>)vulnerabilities);

        public Task<Vulnerability> AddAsync(Vulnerability vulnerability, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(vulnerability));

        public Task<Vulnerability?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<IReadOnlyList<Vulnerability>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<IReadOnlyList<Vulnerability>> GetBySeverityAsync(VulnerabilitySeverity severity, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Vulnerability>>(GetAll().Where(v => v.Severity == severity).ToList());

        public Task<IReadOnlyList<Vulnerability>> GetByStatusAsync(VulnerabilityStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Vulnerability>>(GetAll().Where(v => v.Status == status).ToList());

        public Task<IReadOnlyList<Vulnerability>> GetByTargetIdAsync(string targetId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Vulnerability>>(GetAll().Where(v => v.TargetId == targetId).ToList());

        public Task<Vulnerability> UpdateAsync(Vulnerability vulnerability, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(vulnerability));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="IIncidentRepository"/>.</summary>
    public sealed class InMemoryIncidentRepository : InMemoryRepositoryBase<SecurityIncident>, IIncidentRepository
    {
        public InMemoryIncidentRepository() : base(i => i.Id) { }

        public void Seed(params SecurityIncident[] incidents) => Seed((IEnumerable<SecurityIncident>)incidents);

        public Task<SecurityIncident> AddAsync(SecurityIncident incident, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(incident));

        public Task<SecurityIncident?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<IReadOnlyList<SecurityIncident>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<IReadOnlyList<SecurityIncident>> GetBySeverityAsync(IncidentSeverity severity, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SecurityIncident>>(GetAll().Where(i => i.Severity == severity).ToList());

        public Task<IReadOnlyList<SecurityIncident>> GetByStatusAsync(IncidentStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SecurityIncident>>(GetAll().Where(i => i.Status == status).ToList());

        public Task<SecurityIncident> UpdateAsync(SecurityIncident incident, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(incident));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="IEvidenceRepository"/>.</summary>
    public sealed class InMemoryEvidenceRepository : InMemoryRepositoryBase<Evidence>, IEvidenceRepository
    {
        public InMemoryEvidenceRepository() : base(e => e.Id) { }

        public void Seed(params Evidence[] evidence) => Seed((IEnumerable<Evidence>)evidence);

        public Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(evidence));

        public Task<Evidence?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<IReadOnlyList<Evidence>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<IReadOnlyList<Evidence>> GetByTypeAsync(EvidenceType type, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Evidence>>(GetAll().Where(e => e.Type == type).ToList());

        public Task<IReadOnlyList<Evidence>> GetByStatusAsync(EvidenceStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Evidence>>(GetAll().Where(e => e.Status == status).ToList());

        public Task<Evidence> UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(evidence));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="IHostRepository"/>.</summary>
    public sealed class InMemoryHostRepository : InMemoryRepositoryBase<Host>, IHostRepository
    {
        public InMemoryHostRepository() : base(h => h.Id) { }

        public void Seed(params Host[] hosts) => Seed((IEnumerable<Host>)hosts);

        public Task<Host> AddAsync(Host host, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(host));

        public Task<Host?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<IReadOnlyList<Host>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<IReadOnlyList<Host>> GetByTypeAsync(AssetType type, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Host>>(GetAll().Where(h => h.Type == type).ToList());

        public Task<IReadOnlyList<Host>> GetByStatusAsync(AssetStatus status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Host>>(GetAll().Where(h => h.Status == status).ToList());

        public Task<Host> UpdateAsync(Host host, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(host));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="INetworkRepository"/>.</summary>
    public sealed class InMemoryNetworkRepository : InMemoryRepositoryBase<Network>, INetworkRepository
    {
        public InMemoryNetworkRepository() : base(n => n.Id) { }

        public void Seed(params Network[] networks) => Seed((IEnumerable<Network>)networks);

        public Task<Network> AddAsync(Network network, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(network));

        public Task<Network?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<IReadOnlyList<Network>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<Network> UpdateAsync(Network network, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(network));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="IUserRepository"/>.</summary>
    public sealed class InMemoryUserRepository : InMemoryRepositoryBase<User>, IUserRepository
    {
        public InMemoryUserRepository() : base(u => u.Id) { }

        public void Seed(params User[] users) => Seed((IEnumerable<User>)users);

        public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
            => Task.FromResult(Add(user));

        public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Get(id));

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll().FirstOrDefault(u => u.Username == username));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll().FirstOrDefault(u => u.Email == email));

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(GetAll());

        public Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<User>>(GetAll().Where(u => u.Role == role).ToList());

        public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
            => Task.FromResult(Update(user));

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
            => Task.FromResult(Delete(id));

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Count());
    }

    /// <summary>In-memory <see cref="IAuditLogRepository"/>.</summary>
    public sealed class InMemoryAuditLogRepository : IAuditLogRepository
    {
        public List<AuditLog> Entries { get; } = new();

        public Task<AuditLog> AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            Entries.Add(auditLog);
            return Task.FromResult(auditLog);
        }

        public Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuditLog>>(Entries
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .ToList());

        public Task<IReadOnlyList<AuditLog>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuditLog>>(Entries.Where(a => a.UserId == userId).ToList());

        public Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuditLog>>(Entries
                .Where(a => a.Timestamp >= start && a.Timestamp <= end)
                .ToList());

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Entries.Count);
    }

    /// <summary>In-memory <see cref="IUnitOfWork"/> composed of the fakes above.</summary>
    public sealed class FakeUnitOfWork : IUnitOfWork
    {
        public InMemoryThreatRepository ThreatsRepo { get; } = new();
        public InMemoryVulnerabilityRepository VulnerabilitiesRepo { get; } = new();
        public InMemoryIncidentRepository IncidentsRepo { get; } = new();
        public InMemoryEvidenceRepository EvidenceRepo { get; } = new();
        public InMemoryHostRepository HostsRepo { get; } = new();
        public InMemoryNetworkRepository NetworksRepo { get; } = new();
        public InMemoryUserRepository UsersRepo { get; } = new();
        public InMemoryAuditLogRepository AuditLogsRepo { get; } = new();

        public int CommitCount { get; private set; }

        public IThreatRepository Threats => ThreatsRepo;
        public IVulnerabilityRepository Vulnerabilities => VulnerabilitiesRepo;
        public IIncidentRepository Incidents => IncidentsRepo;
        public IEvidenceRepository Evidence => EvidenceRepo;
        public IHostRepository Hosts => HostsRepo;
        public INetworkRepository Networks => NetworksRepo;
        public IUserRepository Users => UsersRepo;
        public IAuditLogRepository AuditLogs => AuditLogsRepo;

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            CommitCount++;
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
