using Microsoft.EntityFrameworkCore;
using QwestRooms.BLL.Filtering;
using QwestRooms.BLL.Services.Implementation;
using QwestRooms.DAL.Models;
using QwestRooms.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace QwestRooms.Tests;

/// <summary>
/// The regression tests for the defect this rebuild is about.
/// </summary>
/// <remarks>
/// <para>
/// In 2019 the repository handed callers <c>DbSet.AsEnumerable()</c>. That executed
/// <c>SELECT * FROM Rooms</c> immediately, so every filter, sort and page ran in memory over the
/// finished list -- and building a DTO for each row walked its lazily-loaded company, address,
/// country, city, street and images, one query at a time. Measured on the original stack, EF6
/// against SQL Server LocalDB with this same 450-room dataset, rendering the first page of 27
/// rooms cost <b>1,072 SQL commands and about 700 ms</b>.
/// </para>
/// <para>
/// These tests pin the fix at two commands, whatever the size of the catalogue and whatever the
/// filter. They are the reason the property survives the next refactor.
/// </para>
/// </remarks>
public sealed class QueryCountTests(ITestOutputHelper output)
{
    private const int PageSize = 27;

    private readonly ITestOutputHelper _output = output;

    [Fact]
    public async Task Catalogue_FirstPageOf450Rooms_ExecutesExactlyTwoQueries()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var service = new RoomsService(database.Repository<Room>(context));

            var page = await service.GetRoomsAsync(RoomFilter.None, 1, PageSize);

            Assert.Equal(PageSize, page.Items.Count);
            Assert.Equal(450, page.TotalCount);
            Assert.Equal(2, database.Commands.Count);
            _output.WriteLine(database.Commands.Describe());
        }
    }

    [Fact]
    public async Task Catalogue_FilteredByCountry_ExecutesExactlyTwoQueries()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var service = new RoomsService(database.Repository<Room>(context));

            var page = await service.GetRoomsAsync(new RoomFilter { CountryId = 1 }, 1, PageSize);

            Assert.Equal(30, page.TotalCount);
            Assert.Equal(2, database.Commands.Count);
        }
    }

    [Fact]
    public async Task Catalogue_LastPage_ExecutesExactlyTwoQueries()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var service = new RoomsService(database.Repository<Room>(context));

            var page = await service.GetRoomsAsync(RoomFilter.None, 17, PageSize);

            Assert.Equal(17, page.PageNumber);
            Assert.NotEmpty(page.Items);
            Assert.Equal(2, database.Commands.Count);
        }
    }

    /// <summary>
    /// The count must not depend on how much data there is, which is the difference between an
    /// N+1 and a fixed cost. Ten rooms and 450 rooms both cost two commands.
    /// </summary>
    [Fact]
    public async Task Catalogue_QueryCount_DoesNotGrowWithTheCatalogue()
    {
        await using var small = await TestDatabase.CreateAsync();
        await small.SeedAsync(context => Populate(context, roomCount: 10));

        int smallCount;
        var smallContext = small.CreateContext();
        await using (smallContext.ConfigureAwait(false))
        {
            var service = new RoomsService(small.Repository<Room>(smallContext));
            await service.GetRoomsAsync(RoomFilter.None, 1, PageSize);
            smallCount = small.Commands.Count;
        }

        await using var large = await TestDatabase.CreateSeededAsync();

        int largeCount;
        var largeContext = large.CreateContext();
        await using (largeContext.ConfigureAwait(false))
        {
            var service = new RoomsService(large.Repository<Room>(largeContext));
            await service.GetRoomsAsync(RoomFilter.None, 1, PageSize);
            largeCount = large.Commands.Count;
        }

        Assert.Equal(2, smallCount);
        Assert.Equal(smallCount, largeCount);
    }

    /// <summary>
    /// The page must be cut by the database, not by the caller. If <c>Skip</c>/<c>Take</c> ever
    /// stopped being translated, the count above would still be two while the application quietly
    /// read the whole table again.
    /// </summary>
    [Fact]
    public async Task Catalogue_FirstPage_AsksTheDatabaseForOnlyOnePage()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var service = new RoomsService(database.Repository<Room>(context));

            await service.GetRoomsAsync(RoomFilter.None, 2, PageSize);

            var projection = database.Commands.Commands.Last();
            Assert.Contains("LIMIT", projection, StringComparison.Ordinal);
            Assert.Contains("OFFSET", projection, StringComparison.Ordinal);
            Assert.Contains("ORDER BY", projection, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// The filter has to reach the database too: a WHERE clause, not a predicate applied to rows
    /// that have already crossed the wire.
    /// </summary>
    [Fact]
    public async Task Catalogue_Filtered_SendsTheFilterAsAWhereClause()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var service = new RoomsService(database.Repository<Room>(context));

            await service.GetRoomsAsync(new RoomFilter { CountryId = 3, CityId = 12 }, 1, PageSize);

            Assert.All(
                database.Commands.Commands,
                sql => Assert.Contains("WHERE", sql, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// What the 2019 code did, reproduced against the same dataset: read every row, then follow
    /// each row's relationships one at a time. It is here so the comparison in the README is
    /// something the test suite measures rather than something the README asserts.
    /// </summary>
    [Fact]
    public async Task LegacyPattern_ReadEverythingThenWalkNavigations_CostsHundredsOfQueries()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            // SELECT * FROM Rooms -- all 450 of them, to show 27.
            var rooms = await context.Rooms.ToListAsync();

            foreach (var room in rooms)
            {
                await context.Entry(room).Reference(r => r.Company).LoadAsync();
                await context.Entry(room).Reference(r => r.Address).LoadAsync();
                await context.Entry(room.Address).Reference(a => a.Country).LoadAsync();
                await context.Entry(room.Address).Reference(a => a.City).LoadAsync();
                await context.Entry(room.Address).Reference(a => a.Street).LoadAsync();
                await context.Entry(room).Collection(r => r.Images).LoadAsync();
            }

            var legacy = database.Commands.Count;
            _output.WriteLine($"2019 pattern: {legacy} SQL commands for one page of {PageSize} rooms.");

            Assert.Equal(450, rooms.Count);
            Assert.True(legacy > 400, $"expected hundreds of commands, measured {legacy}");
        }
    }

    private static void Populate(QwestRooms.DAL.RoomsContext context, int roomCount)
    {
        var country = TestData.Country(1, "Ukraine");
        var city = TestData.City(1, "Kyiv");
        var street = TestData.Street(1, "Khreshchatyk");
        var company = TestData.Company(1, "Cipher Escape Rooms");
        var address = TestData.Address(1, country, city, street);

        context.Countries.Add(country);
        context.Cities.Add(city);
        context.Streets.Add(street);
        context.Companies.Add(company);
        context.Addresses.Add(address);

        for (var id = 1; id <= roomCount; id++)
        {
            context.Rooms.Add(TestData.Room(id, address, company));
        }
    }
}
