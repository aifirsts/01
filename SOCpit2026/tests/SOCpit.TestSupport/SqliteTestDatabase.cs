// SOCpit - Security Operations Center
// SQLite (in-memory) test harness shared by storage, import and integration tests.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SOCpit.Storage;
using System;

namespace SOCpit.TestSupport
{
    /// <summary>
    /// Owns a single open in-memory SQLite connection and hands out DbContext instances
    /// bound to it, so every context in a test sees the same database.
    /// </summary>
    public sealed class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;
        private bool _disposed;

        private SqliteTestDatabase(SqliteConnection connection)
        {
            _connection = connection;
        }

        /// <summary>Creates a new in-memory database with the full SOCpit schema applied.</summary>
        public static SqliteTestDatabase Create()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var database = new SqliteTestDatabase(connection);
            using var context = database.CreateContext();
            context.Database.EnsureCreated();
            return database;
        }

        /// <summary>Creates a DbContext bound to this in-memory database.</summary>
        public SOCpitDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<SOCpitDbContext>()
                .UseSqlite(_connection)
                .Options;

            return new SOCpitDbContext(options);
        }

        /// <summary>Creates a <see cref="UnitOfWork"/> over a fresh context on this database.</summary>
        public UnitOfWork CreateUnitOfWork() => new(CreateContext());

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _connection.Dispose();
        }
    }
}
