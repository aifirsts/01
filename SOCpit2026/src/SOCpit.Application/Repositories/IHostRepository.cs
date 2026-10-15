// SOCpit - Security Operations Center
// Host repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
    /// <summary>Repository interface for hosts.</summary>
    public interface IHostRepository
    {
        Task<Host> AddAsync(Host host, CancellationToken cancellationToken = default);
        Task<Host?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Host>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Host>> GetByTypeAsync(AssetType type, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Host>> GetByStatusAsync(AssetStatus status, CancellationToken cancellationToken = default);
        Task<Host> UpdateAsync(Host host, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }
}
