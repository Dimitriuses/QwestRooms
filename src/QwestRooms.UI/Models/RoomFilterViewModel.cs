using QwestRooms.BLL.Filtering;

namespace QwestRooms.UI.Models;

/// <summary>
/// The applied filter, model-bound from the query string. It is a separate type from
/// <see cref="RoomFilter"/> so that what arrives from a URL and what the business layer accepts can
/// diverge without one dragging the other along.
/// </summary>
public sealed class RoomFilterViewModel
{
    public int? CountryId { get; set; }

    public int? CityId { get; set; }

    public int? AddressId { get; set; }

    public RoomFilter ToFilter() => new()
    {
        CountryId = CountryId,
        CityId = CityId,
        AddressId = AddressId
    };
}
