namespace QwestRooms.BLL.Dtos;

/// <summary>One room as the catalogue shows it: everything a card needs and nothing else.</summary>
/// <remarks>
/// The properties are <c>get; set;</c> rather than <c>init</c> because these objects are only ever
/// constructed inside an <see cref="System.Linq.Expressions.Expression"/> object initializer, which
/// is what lets Entity Framework turn the whole graph into the SELECT list of one statement. See
/// <see cref="Mapping.Projections"/>.
/// </remarks>
public sealed record RoomDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TimeSpan TimeToPass { get; set; }

    public int MinPlayers { get; set; }

    public int MaxPlayers { get; set; }

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Rating { get; set; }

    public int FearLevel { get; set; }

    public int Difficulty { get; set; }

    public string LogoPath { get; set; } = string.Empty;

    public AddressDto Address { get; set; } = new();

    public CompanyDto Company { get; set; } = new();

    public List<ImageDto> Images { get; set; } = [];
}
