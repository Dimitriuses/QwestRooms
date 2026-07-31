using QwestRooms.BLL.Dtos;
using QwestRooms.BLL.Filtering;

namespace QwestRooms.UI.Models;

/// <summary>
/// The rooms grid on its own: one page of rooms, its pager state, and the filter that produced it.
/// </summary>
/// <remarks>
/// The filter travels with the page so every pager link can carry the criteria forward. That is
/// what lets filtering and paging combine, which the 2019 <c>Session</c>-based version could not.
/// </remarks>
public sealed class RoomListViewModel
{
    public required PagedResult<RoomDto> Page { get; init; }

    public required RoomFilterViewModel Filter { get; init; }
}
