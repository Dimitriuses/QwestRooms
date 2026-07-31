namespace QwestRooms.DAL.Models;

/// <summary>One escape-quest room, offered by a company at a single address.</summary>
/// <remarks>
/// The navigation properties are deliberately <em>not</em> <c>virtual</c>, and lazy loading is not
/// enabled anywhere. Reading <c>room.Company</c> on an entity that was loaded without it now gives
/// null rather than quietly issuing another query -- which is what turned a single page of this
/// catalogue into 1,072 database round trips in the 2019 version.
/// </remarks>
public class Room
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    /// <summary>Advertised time to escape. Stored as text by SQLite, e.g. <c>01:15:00</c>.</summary>
    public TimeSpan TimeToPass { get; set; }

    public int MinPlayers { get; set; }

    public int MaxPlayers { get; set; }

    public required string Phone { get; set; }

    public required string Email { get; set; }

    /// <summary>Player rating, 1-10.</summary>
    public int Rating { get; set; }

    /// <summary>How frightening the room is, 1-5.</summary>
    public int FearLevel { get; set; }

    /// <summary>How hard the room is, 1-5.</summary>
    public int Difficulty { get; set; }

    /// <summary>Site-relative path to the poster image, e.g. <c>/img/rooms/bunker.svg</c>.</summary>
    public required string LogoPath { get; set; }

    public int AddressId { get; set; }

    public Address Address { get; set; } = null!;

    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public ICollection<Image> Images { get; } = new List<Image>();
}
