// SOCpit - Security Operations Center
// Evidence repository interface
// Based on MATRIX V3 with repository pattern

using SOCpit.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Application.Repositories
{
    /// <summary>Repository interface for evidence.</summary>
    public interface IEvidenceRepository
    {
        Task<Evidence> AddAsync(Evidence evidence, CancellationToken cancellationToken = default);
        Task<Evidence?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Evidence>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Evidence>> GetByTypeAsync(EvidenceType type, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Evidence>> GetByStatusAsync(EvidenceStatus status, CancellationToken cancellationToken = default);
        Task<Evidence> UpdateAsync(Evidence evidence, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
    }
}
