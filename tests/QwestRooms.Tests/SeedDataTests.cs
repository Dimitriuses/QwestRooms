using Microsoft.EntityFrameworkCore;
using QwestRooms.Tests.Infrastructure;
using Xunit;

namespace QwestRooms.Tests;

/// <summary>
/// The demo dataset is part of the product here -- it is what a reader sees on first run -- so it
/// gets the same treatment as code.
/// </summary>
/// <remarks>
/// The 2019 data was generated without constraints and showed it: ratings and difficulty on a
/// 1-100 scale in a UI that implied 1-10 and 1-5, escape rooms lasting up to twenty hours, and
/// cities filed under countries they do not belong to. The assertions below are those defects,
/// turned into rules.
/// </remarks>
public sealed class SeedDataTests
{
    [Fact]
    public async Task Seed_LoadsTheWholeCatalogue()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            Assert.Equal(15, await context.Countries.CountAsync());
            Assert.Equal(69, await context.Cities.CountAsync());
            Assert.Equal(69, await context.Streets.CountAsync());
            Assert.Equal(18, await context.Companies.CountAsync());
            Assert.Equal(450, await context.Addresses.CountAsync());
            Assert.Equal(450, await context.Rooms.CountAsync());
            Assert.Equal(450, await context.Images.CountAsync());
        }
    }

    [Fact]
    public async Task Seed_IsIdempotentThroughInitialise()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var rooms = await QwestRooms.DAL.Seeding.DatabaseSeeder.InitialiseAsync(context);

            Assert.Equal(450, rooms);
        }
    }

    [Fact]
    public async Task Seed_ScoresAreInTheRangeTheCardsImply()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var scores = await context.Rooms
                .Select(room => new { room.Rating, room.FearLevel, room.Difficulty })
                .ToListAsync();

            Assert.All(scores, score =>
            {
                Assert.InRange(score.Rating, 1, 10);      // the card shows this out of 10
                Assert.InRange(score.FearLevel, 1, 5);
                Assert.InRange(score.Difficulty, 1, 5);
            });
        }
    }

    [Fact]
    public async Task Seed_RoomsLastBetween45And90Minutes()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var durations = await context.Rooms.Select(room => room.TimeToPass).ToListAsync();

            Assert.All(durations, duration => Assert.InRange(duration.TotalMinutes, 45, 90));
        }
    }

    [Fact]
    public async Task Seed_EveryRoomTakesAtLeastTwoPlayersAndAnUpperBoundAboveTheLower()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var parties = await context.Rooms
                .Select(room => new { room.MinPlayers, room.MaxPlayers })
                .ToListAsync();

            Assert.All(parties, party =>
            {
                Assert.InRange(party.MinPlayers, 2, 3);
                Assert.True(party.MaxPlayers > party.MinPlayers);
            });
        }
    }

    /// <summary>
    /// Every address must pair a city with the country that city is actually in. In 2019 they were
    /// drawn independently, so the catalogue offered rooms in Prague, Spain.
    /// </summary>
    [Fact]
    public async Task Seed_EveryCityBelongsToExactlyOneCountry()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var countriesPerCity = await context.Addresses
                .GroupBy(address => address.CityId)
                .Select(group => new { CityId = group.Key, Countries = group.Select(a => a.CountryId).Distinct().Count() })
                .ToListAsync();

            Assert.Equal(69, countriesPerCity.Count);
            Assert.All(countriesPerCity, city => Assert.Equal(1, city.Countries));
        }
    }

    /// <summary>
    /// No country lists the same room concept twice, so no page of filtered results repeats
    /// itself. The same concept appearing under another country is intended: an escape-room chain
    /// runs one room at several locations.
    /// </summary>
    [Fact]
    public async Task Seed_NoCountryListsTheSameRoomTwice()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var duplicates = await context.Rooms
                .GroupBy(room => new { room.Address.CountryId, room.Name })
                .Where(group => group.Count() > 1)
                .Select(group => group.Key.Name)
                .ToListAsync();

            Assert.Empty(duplicates);
        }
    }

    /// <summary>
    /// Every poster the seed data points at is a file this repository actually ships. The 2019
    /// dataset hotlinked real escape-room websites, roughly two thirds of which are now gone.
    /// </summary>
    [Fact]
    public async Task Seed_EveryPosterExistsInWwwroot()
    {
        await using var database = await TestDatabase.CreateSeededAsync();
        var context = database.CreateContext();
        await using (context.ConfigureAwait(false))
        {
            var paths = await context.Rooms.Select(room => room.LogoPath).Distinct().ToListAsync();
            var wwwroot = LocateWwwroot();

            Assert.NotEmpty(paths);
            Assert.All(paths, path =>
            {
                Assert.StartsWith("/img/rooms/", path, StringComparison.Ordinal);
                Assert.True(
                    File.Exists(Path.Combine(wwwroot, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar))),
                    $"{path} is referenced by the seed data but is not in wwwroot");
            });
        }
    }

    private static string LocateWwwroot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "QwestRooms.UI", "wwwroot");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find src/QwestRooms.UI/wwwroot above " + AppContext.BaseDirectory);
    }
}
