using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QwestRooms.DAL;

/// <summary>
/// Lets <c>dotnet ef migrations add</c> build a context without starting the web application.
/// The connection string here is never opened -- the tooling only needs the provider, so that it
/// knows which SQL dialect to scaffold.
/// </summary>
public sealed class DesignTimeRoomsContextFactory : IDesignTimeDbContextFactory<RoomsContext>
{
    public RoomsContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<RoomsContext>()
            .UseSqlite("Data Source=design-time.db")
            .Options;

        return new RoomsContext(options);
    }
}
