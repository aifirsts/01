// SOCpit - Security Operations Center
// Network repository implementation
// Based on MATRIX V3 with repository pattern

using SOCpit.Application.Repositories;
using SOCpit.Core;

namespace SOCpit.Storage.Repositories
{
    /// <summary>Repository implementation for networks.</summary>
    public sealed class NetworkRepository : BaseRepository<Network>, INetworkRepository
    {
        public NetworkRepository(SOCpitDbContext dbContext) : base(dbContext)
        {
        }
    }
}
