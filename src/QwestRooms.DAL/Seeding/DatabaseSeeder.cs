using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace QwestRooms.DAL.Seeding;

/// <summary>
/// Brings a database up to date and, if the catalogue is empty, loads the demo dataset.
/// </summary>
/// <remarks>
/// <para>
/// The 2019 version left this to an EF6 <c>DropCreateDatabaseIfModelChanges</c> initializer, which
/// meant the schema was created by whichever HTTP request happened to arrive first and could never
/// be evolved -- a model change threw the data away. This applies migrations instead, so the
/// schema has a history and an upgrade path, and it runs once at startup rather than inside a
/// request.
/// </para>
/// <para>
/// The seed scripts are embedded resources of this assembly, so they are found identically by the
/// web application, by the test suite and by a published single-folder build.
/// </para>
/// </remarks>
public static class DatabaseSeeder
{
    /// <summary>
    /// Ordered so that a table's dependencies are always populated first. The scripts insert
    /// without explicit primary keys and rely on the identity values starting at 1, so the order
    /// is load-bearing.
    /// </summary>
    private static readonly string[] ScriptNames =
    [
        "Countries.sql",
        "Cities.sql",
        "Streets.sql",
        "Companies.sql",
        "Addresses.sql",
        "Rooms.sql",
        "Images.sql"
    ];

    /// <summary>Applies any pending migrations, then seeds the catalogue if it is empty.</summary>
    /// <returns>The number of rooms in the catalogue afterwards.</returns>
    public static async Task<int> InitialiseAsync(RoomsContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (!await context.Rooms.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            await SeedAsync(context, cancellationToken).ConfigureAwait(false);
        }

        return await context.Rooms.CountAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Loads the demo dataset unconditionally, in one transaction.</summary>
    public static async Task SeedAsync(RoomsContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        await using (transaction.ConfigureAwait(false))
        {
            foreach (var scriptName in ScriptNames)
            {
                var sql = ReadScript(scriptName);
                await context.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static string ReadScript(string scriptName)
    {
        var assembly = typeof(DatabaseSeeder).GetTypeInfo().Assembly;
        var resourceName = "QwestRooms.DAL.MockData." + scriptName;

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Seed script '{resourceName}' is not embedded in {assembly.GetName().Name}. " +
                "Check the EmbeddedResource item in QwestRooms.DAL.csproj.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
