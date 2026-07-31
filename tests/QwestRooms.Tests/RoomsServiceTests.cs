using QwestRooms.BLL.Filtering;
using QwestRooms.BLL.Services.Implementation;
using QwestRooms.DAL;
using QwestRooms.DAL.Models;
using QwestRooms.Tests.Infrastructure;
using Xunit;

namespace QwestRooms.Tests;

/// <summary>
/// Covers the filtering and paging that used to live in the controller as three near-identical
/// branches of nested foreach loops over an already-materialised list.
/// </summary>
public sealed class RoomsServiceTests
{
    // Ids only. The entities themselves are built fresh for each test: EF fixup writes tracked
    // children into a parent's navigation collection, so a shared static Country would accumulate
    // every previous test's addresses and the next Add would fail on duplicate keys.
    private const int UkraineId = 1;
    private const int PolandId = 2;
    private const int KyivId = 10;
    private const int LvivId = 11;
    private const int WarsawId = 12;

    private const int KyivAddressA = 1000;
    private const int KyivAddressB = 1001;
    private const int LvivAddress = 1002;
    private const int WarsawAddress = 1003;

    [Fact]
    public async Task GetRooms_WithoutFilter_ReturnsEverything()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, RoomFilter.None, 1, 10);

        Assert.Equal(4, page.TotalCount);
        Assert.Equal(4, page.Items.Count);
    }

    [Fact]
    public async Task GetRooms_FiltersByCountry()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, new RoomFilter { CountryId = UkraineId }, 1, 10);

        Assert.Equal(3, page.TotalCount);
        Assert.All(page.Items, room => Assert.Equal("Ukraine", room.Address.Country.Name));
    }

    [Fact]
    public async Task GetRooms_FiltersByCountryAndCity()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(
            database,
            new RoomFilter { CountryId = UkraineId, CityId = KyivId },
            1,
            10);

        Assert.Equal(2, page.TotalCount);
        Assert.All(page.Items, room => Assert.Equal("Kyiv", room.Address.City.Name));
    }

    /// <summary>
    /// A deliberately contradictory filter: the address is in Poland while the country and city
    /// say Ukraine and Kyiv. The narrowest criterion is meant to win outright.
    /// </summary>
    [Fact]
    public async Task GetRooms_AddressId_TakesPrecedenceOverCountryAndCity()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(
            database,
            new RoomFilter { CountryId = UkraineId, CityId = KyivId, AddressId = WarsawAddress },
            1,
            10);

        Assert.Equal(1, page.TotalCount);
        Assert.Equal(WarsawAddress, Assert.Single(page.Items).Address.Id);
    }

    [Fact]
    public async Task GetRooms_TotalCount_CountsAllMatches_NotJustThePage()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, RoomFilter.None, 1, 2);

        Assert.Equal(4, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
    }

    [Fact]
    public async Task GetRooms_SecondPage_ReturnsTheNextSlice()
    {
        await using var database = await CreateSampleAsync();

        var first = await QueryAsync(database, RoomFilter.None, 1, 2);
        var second = await QueryAsync(database, RoomFilter.None, 2, 2);

        Assert.Equal([1, 2], first.Items.Select(room => room.Id));
        Assert.Equal([3, 4], second.Items.Select(room => room.Id));
    }

    [Fact]
    public async Task GetRooms_PageBeyondTheEnd_ReturnsEmptyButKeepsTotalCount()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, RoomFilter.None, 99, 2);

        Assert.Empty(page.Items);
        Assert.Equal(4, page.TotalCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetRooms_ClampsPageNumberToOne(int requestedPage)
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, RoomFilter.None, requestedPage, 2);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal([1, 2], page.Items.Select(room => room.Id));
    }

    [Fact]
    public async Task GetRooms_ClampsPageSizeToOne()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, RoomFilter.None, 1, 0);

        Assert.Equal(1, page.PageSize);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task GetRooms_ProjectsTheWholeCardIncludingNestedNamesAndImages()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, new RoomFilter { AddressId = KyivAddressA }, 1, 10);

        var room = Assert.Single(page.Items);
        Assert.Equal(2, room.MinPlayers);
        Assert.Equal(4, room.Difficulty);
        Assert.Equal("Cipher Escape Rooms", room.Company.Name);
        Assert.Equal("Kyiv", room.Address.City.Name);
        Assert.Equal("Ukraine", room.Address.Country.Name);
        Assert.Equal("Main", room.Address.Street.Name);
        Assert.Equal("/img/rooms/space.svg", Assert.Single(room.Images).Path);
    }

    [Fact]
    public async Task GetRooms_ReturnsEmpty_WhenNothingMatches()
    {
        await using var database = await CreateSampleAsync();
        var page = await QueryAsync(database, new RoomFilter { CountryId = 9999 }, 1, 10);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal(0, page.TotalPages);
    }

    private static async Task<PagedResult<QwestRooms.BLL.Dtos.RoomDto>> QueryAsync(
        TestDatabase database,
        RoomFilter filter,
        int pageNumber,
        int pageSize)
    {
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var service = new RoomsService(database.Repository<Room>(context));
            return await service.GetRoomsAsync(filter, pageNumber, pageSize);
        }
    }

    private static async Task<TestDatabase> CreateSampleAsync()
    {
        var database = await TestDatabase.CreateAsync();
        await database.SeedAsync(Populate);
        return database;
    }

    private static void Populate(RoomsContext context)
    {
        var ukraine = TestData.Country(UkraineId, "Ukraine");
        var poland = TestData.Country(PolandId, "Poland");
        var kyiv = TestData.City(KyivId, "Kyiv");
        var lviv = TestData.City(LvivId, "Lviv");
        var warsaw = TestData.City(WarsawId, "Warsaw");
        var main = TestData.Street(100, "Main");
        var company = TestData.Company(200, "Cipher Escape Rooms");

        context.Countries.AddRange(ukraine, poland);
        context.Cities.AddRange(kyiv, lviv, warsaw);
        context.Streets.Add(main);
        context.Companies.Add(company);

        var kyivA = TestData.Address(KyivAddressA, ukraine, kyiv, main, "1");
        var kyivB = TestData.Address(KyivAddressB, ukraine, kyiv, main, "2");
        var lvivAddress = TestData.Address(LvivAddress, ukraine, lviv, main, "3");
        var warsawAddress = TestData.Address(WarsawAddress, poland, warsaw, main, "4");
        context.Addresses.AddRange(kyivA, kyivB, lvivAddress, warsawAddress);

        var first = TestData.Room(1, kyivA, company);
        context.Rooms.AddRange(
            first,
            TestData.Room(2, kyivB, company),
            TestData.Room(3, lvivAddress, company),
            TestData.Room(4, warsawAddress, company));

        context.Images.Add(TestData.Image(1, first, "/img/rooms/space.svg"));
    }
}
