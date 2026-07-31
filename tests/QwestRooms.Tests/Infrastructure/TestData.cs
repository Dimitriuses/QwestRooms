using QwestRooms.DAL.Models;

namespace QwestRooms.Tests.Infrastructure;

/// <summary>
/// Builds small, readable object graphs for the service tests. Ids are assigned explicitly so a
/// test can assert on them without first reading them back.
/// </summary>
internal static class TestData
{
    public static Country Country(int id, string name) => new() { Id = id, Name = name };

    public static City City(int id, string name) => new() { Id = id, Name = name };

    public static Street Street(int id, string name) => new() { Id = id, Name = name };

    public static Company Company(int id, string name) => new() { Id = id, Name = name };

    public static Address Address(int id, Country country, City city, Street street, string houseNumber = "1") =>
        new()
        {
            Id = id,
            HouseNumber = houseNumber,
            CountryId = country.Id,
            CityId = city.Id,
            StreetId = street.Id
        };

    public static Room Room(int id, Address address, Company company, string? name = null) =>
        new()
        {
            Id = id,
            Name = name ?? $"Room {id}",
            Description = $"Description {id}",
            TimeToPass = TimeSpan.FromMinutes(60),
            MinPlayers = 2,
            MaxPlayers = 6,
            Phone = $"555-000{id}",
            Email = $"room{id}@qwestrooms.example",
            Rating = 5,
            FearLevel = 3,
            Difficulty = 4,
            LogoPath = $"/img/rooms/room{id}.svg",
            AddressId = address.Id,
            CompanyId = company.Id
        };

    public static Image Image(int id, Room room, string path) =>
        new() { Id = id, Path = path, RoomId = room.Id };
}
