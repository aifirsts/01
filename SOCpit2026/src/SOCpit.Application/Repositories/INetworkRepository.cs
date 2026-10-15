// SOCpit - Security Operations Center
// Network repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
    /// <summary>Repository interface for networks.</summary>
    public interface INetworkRepository
    {
        Task<Network> AddAsync(Network network, CancellationToken cancellationToken = default);
        Task<Network?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Network>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Network> UpdateAsync(Network network, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }
}
