namespace QwestRooms.BLL.Filtering;

/// <summary>
/// The criteria a room list can be narrowed by. Every property is optional; a filter with all of
/// them null matches everything.
/// </summary>
/// <remarks>
/// This replaces an arrangement where the selected country, city and address were kept in
/// <c>Session</c> and read back inside the controller. Passing them explicitly is what lets a
/// filtered list be linked to, bookmarked, opened in two tabs with different filters, and paged
/// through without losing the filter.
/// </remarks>
public sealed record RoomFilter
{
    public int? CountryId { get; init; }

    public int? CityId { get; init; }

    public int? AddressId { get; init; }

    /// <summary>The filter that matches every room.</summary>
    public static RoomFilter None { get; } = new();

    /// <summary>True when no criterion has been chosen.</summary>
    public bool IsEmpty => CountryId is null && CityId is null && AddressId is null;
}
