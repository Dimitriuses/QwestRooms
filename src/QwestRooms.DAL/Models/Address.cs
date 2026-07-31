namespace QwestRooms.DAL.Models;

/// <summary>
/// A street address. Country, city and street are separate rows rather than columns so the
/// catalogue's country -> city -> address filter can offer each level's real options.
/// </summary>
public class Address
{
    public int Id { get; set; }

    public required string HouseNumber { get; set; }

    public int CityId { get; set; }

    public City City { get; set; } = null!;

    public int CountryId { get; set; }

    public Country Country { get; set; } = null!;

    public int StreetId { get; set; }

    public Street Street { get; set; } = null!;

    public ICollection<Room> Rooms { get; } = new List<Room>();
}
