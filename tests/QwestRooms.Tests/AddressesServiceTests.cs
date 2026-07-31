using QwestRooms.BLL.Services.Implementation;
using QwestRooms.DAL;
using QwestRooms.DAL.Models;
using QwestRooms.Tests.Infrastructure;
using Xunit;

namespace QwestRooms.Tests;

/// <summary>
/// Covers the queries behind the country -> city -> address filter. The de-duplication assertions
/// are the point: the 2019 code de-duplicated countries with a quadratic loop over a HashSet, and
/// did not de-duplicate cities at all, so a country with several addresses in one city offered
/// that city several times.
/// </summary>
public sealed class AddressesServiceTests
{
    // Ids only. The entities are built fresh for each test: EF fixup writes tracked children into
    // a parent's navigation collection, so a shared static Country would accumulate every previous
    // test's addresses and the next Add would fail on duplicate keys.
    private const int UkraineId = 1;
    private const int PolandId = 2;
    private const int KyivId = 10;
    private const int LvivId = 11;
    private const int WarsawId = 12;

    [Fact]
    public async Task GetCountries_ReturnsEachCountryOnce()
    {
        await using var database = await CreateSampleAsync();
        var countries = await RunAsync(database, service => service.GetCountriesAsync());

        Assert.Equal(2, countries.Count);
        Assert.Equal(["Poland", "Ukraine"], countries.Select(country => country.Name));
    }

    [Fact]
    public async Task GetCountries_IsOrderedByName()
    {
        await using var database = await CreateSampleAsync();
        var names = (await RunAsync(database, service => service.GetCountriesAsync()))
            .Select(country => country.Name)
            .ToArray();

        Assert.Equal(names.OrderBy(name => name, StringComparer.Ordinal), names);
    }

    [Fact]
    public async Task GetCitiesByCountry_ReturnsEachCityOnce()
    {
        await using var database = await CreateSampleAsync();

        // Kyiv appears in two of Ukraine's addresses but must be offered once.
        var cities = await RunAsync(database, service => service.GetCitiesByCountryAsync(UkraineId));

        Assert.Equal(2, cities.Count);
        Assert.Equal(["Kyiv", "Lviv"], cities.Select(city => city.Name));
    }

    [Fact]
    public async Task GetCitiesByCountry_ExcludesOtherCountries()
    {
        await using var database = await CreateSampleAsync();
        var cities = await RunAsync(database, service => service.GetCitiesByCountryAsync(PolandId));

        Assert.Equal("Warsaw", Assert.Single(cities).Name);
    }

    [Fact]
    public async Task GetCitiesByCountry_ReturnsEmpty_ForUnknownCountry()
    {
        await using var database = await CreateSampleAsync();

        Assert.Empty(await RunAsync(database, service => service.GetCitiesByCountryAsync(9999)));
    }

    [Fact]
    public async Task GetAddressesByCountryAndCity_ReturnsOnlyMatches()
    {
        await using var database = await CreateSampleAsync();
        var addresses = await RunAsync(
            database,
            service => service.GetAddressesByCountryAndCityAsync(UkraineId, KyivId));

        Assert.Equal(2, addresses.Count);
        Assert.All(addresses, address => Assert.Equal("Kyiv", address.City.Name));
    }

    [Fact]
    public async Task GetAddressesByCountryAndCity_IsOrderedByStreetThenHouseNumber()
    {
        await using var database = await CreateSampleAsync();
        var addresses = await RunAsync(
            database,
            service => service.GetAddressesByCountryAndCityAsync(UkraineId, KyivId));

        Assert.Equal(["Main", "Oak"], addresses.Select(address => address.Street.Name));
    }

    [Fact]
    public async Task GetAddressesByCountryAndCity_ProjectsTheWholeAddress()
    {
        await using var database = await CreateSampleAsync();
        var addresses = await RunAsync(
            database,
            service => service.GetAddressesByCountryAndCityAsync(PolandId, WarsawId));

        var address = Assert.Single(addresses);
        Assert.Equal("4", address.HouseNumber);
        Assert.Equal("Warsaw", address.City.Name);
        Assert.Equal("Poland", address.Country.Name);
        Assert.Equal("Main", address.Street.Name);
    }

    private static async Task<T> RunAsync<T>(TestDatabase database, Func<AddressesService, Task<T>> query)
    {
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            return await query(new AddressesService(database.Repository<Address>(context)));
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
        var oak = TestData.Street(101, "Oak");

        context.Countries.AddRange(ukraine, poland);
        context.Cities.AddRange(kyiv, lviv, warsaw);
        context.Streets.AddRange(main, oak);

        context.Addresses.AddRange(
            TestData.Address(1, ukraine, kyiv, main, "1"),
            TestData.Address(2, ukraine, kyiv, oak, "2"),   // same country and city as above
            TestData.Address(3, ukraine, lviv, main, "3"),
            TestData.Address(4, poland, warsaw, main, "4"));
    }
}
