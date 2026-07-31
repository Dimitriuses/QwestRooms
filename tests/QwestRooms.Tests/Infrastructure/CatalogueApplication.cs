using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace QwestRooms.Tests.Infrastructure;

/// <summary>
/// Starts the real application -- its DI container, middleware pipeline, routing, Razor views and
/// startup migration -- against a throwaway SQLite file.
/// </summary>
/// <remarks>
/// Nothing is stubbed. The point of these tests is that the pieces are wired together, which is
/// the one thing unit tests cannot tell you and the thing that was actually broken in 2019: the
/// default route pointed at a controller that did not exist, and the account controller threw
/// before reaching any of its actions.
/// </remarks>
public sealed class CatalogueApplication : WebApplicationFactory<Program>
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"qwestrooms-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Pooling off so the file handle is released when the host shuts down and the temporary
        // database can be deleted; a pooled SQLite connection outlives the context that opened it.
        builder.UseSetting("ConnectionStrings:RoomsContext", $"Data Source={_databasePath};Pooling=False");
        builder.UseEnvironment("Production");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing)
        {
            return;
        }

        foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
        {
            var path = _databasePath + suffix;
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
