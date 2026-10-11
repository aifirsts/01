// SOCpit - Security Operations Center
// Unit of work implementation
// Based on MATRIX V3 with unit of work pattern

using SOCpit.Application.Repositories;
using SOCpit.Storage.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Storage
{
    /// <summary>Unit of work implementation for coordinating multiple repositories.</summary>
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly SOCpitDbContext _dbContext;
        private bool _disposed = false;

        public UnitOfWork(SOCpitDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private IThreatRepository? _threats;
        public IThreatRepository Threats => _threats ??= new ThreatRepository(_dbContext);

        private IVulnerabilityRepository? _vulnerabilities;
        public IVulnerabilityRepository Vulnerabilities => _vulnerabilities ??= new VulnerabilityRepository(_dbContext);

        private IIncidentRepository? _incidents;
        public IIncidentRepository Incidents => _incidents ??= new IncidentRepository(_dbContext);

        private IEvidenceRepository? _evidence;
        public IEvidenceRepository Evidence => _evidence ??= new EvidenceRepository(_dbContext);

        private IHostRepository? _hosts;
        public IHostRepository Hosts => _hosts ??= new HostRepository(_dbContext);

        private INetworkRepository? _networks;
        public INetworkRepository Networks => _networks ??= new NetworkRepository(_dbContext);

        private IUserRepository? _users;
        public IUserRepository Users => _users ??= new UserRepository(_dbContext);

        private IAuditLogRepository? _auditLogs;
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_dbContext);

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _dbContext.Dispose();
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _dbContext.DisposeAsync();
                _disposed = true;
            }
        }
    }
}
