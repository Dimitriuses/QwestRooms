using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QwestRooms.DAL;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;
using QwestRooms.DAL.Seeding;

namespace QwestRooms.Tests.Infrastructure;

/// <summary>
/// A real SQLite database, created by the application's own migrations and held in memory for the
/// lifetime of one test.
/// </summary>
/// <remarks>
/// <para>
/// These tests deliberately do not mock the repository. The property most of them are about --
/// that filtering, ordering, paging and mapping are translated into SQL instead of running in
/// memory -- is invisible to a mock, because LINQ to Objects will happily execute a query no
/// database could. That is exactly the mistake this codebase used to make. Running against a real
/// provider means an untranslatable query fails a test rather than passing one.
/// </para>
/// <para>
/// SQLite keeps an in-memory database alive only while a connection to it is open, so this owns
/// one and hands it to every context it creates. Creating and migrating one costs a few
/// milliseconds, so each test gets its own.
/// </para>
/// </remarks>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private TestDatabase(SqliteConnection connection) => _connection = connection;

    /// <summary>Every SQL command executed through a context this database created.</summary>
    public CommandCountingInterceptor Commands { get; } = new();

    /// <summary>An empty database with the current schema.</summary>
    public static async Task<TestDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync().ConfigureAwait(false);

        var database = new TestDatabase(connection);

        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            // The application's migrations, not EnsureCreated: if a migration would not apply,
            // that is a failing test rather than a surprise on the next deployment.
            await context.Database.MigrateAsync().ConfigureAwait(false);
        }

        database.Commands.Reset();
        return database;
    }

    /// <summary>A database carrying the repository's demo dataset: 450 rooms in 15 countries.</summary>
    public static async Task<TestDatabase> CreateSeededAsync()
    {
        var database = await CreateAsync().ConfigureAwait(false);

        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            await DatabaseSeeder.SeedAsync(context).ConfigureAwait(false);
        }

        database.Commands.Reset();
        return database;
    }

    public RoomsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RoomsContext>()
            .UseSqlite(_connection)
            .AddInterceptors(Commands)
            .Options;

        return new RoomsContext(options);
    }

    public IGenericRepository<TEntity> Repository<TEntity>(RoomsContext context)
        where TEntity : class => new GenericRepository<TEntity>(context);

    /// <summary>Adds entities and saves them, then forgets the commands it took to do it.</summary>
    public async Task SeedAsync(Action<RoomsContext> populate)
    {
        ArgumentNullException.ThrowIfNull(populate);

        var context = CreateContext();
        await using (context.ConfigureAwait(false))
        {
            populate(context);
            await context.SaveChangesAsync().ConfigureAwait(false);
        }

        Commands.Reset();
    }

    public async ValueTask DisposeAsync() => await _connection.DisposeAsync().ConfigureAwait(false);
}
