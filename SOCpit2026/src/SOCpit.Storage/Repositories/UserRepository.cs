// SOCpit - Security Operations Center
// User repository implementation
// Based on MATRIX V3 with repository pattern

using Microsoft.EntityFrameworkCore;
using SOCpit.Application.Repositories;
using SOCpit.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOCpit.Storage.Repositories
{
    /// <summary>Repository implementation for users.</summary>
    public sealed class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SOCpitDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .Where(u => u.Role == role)
                .ToListAsync(cancellationToken);
        }
    }
}
