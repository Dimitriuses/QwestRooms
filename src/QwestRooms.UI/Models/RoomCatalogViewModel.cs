using QwestRooms.BLL.Dtos;

namespace QwestRooms.UI.Models;

/// <summary>The full catalogue page: the rooms grid plus the filter's top-level options.</summary>
public sealed class RoomCatalogViewModel
{
    public required RoomListViewModel List { get; init; }

    public required IReadOnlyList<CountryDto> Countries { get; init; }
}
