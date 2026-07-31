using QwestRooms.BLL.Dtos;
using QwestRooms.BLL.Filtering;

namespace QwestRooms.BLL.Services.Abstraction;

public interface IRoomsService
{
    /// <summary>
    /// Returns one page of rooms matching <paramref name="filter"/>, together with the total number
    /// of matches. Filtering, ordering and paging all happen in the database.
    /// </summary>
    /// <param name="filter">Criteria to narrow by; <see cref="RoomFilter.None"/> matches everything.</param>
    /// <param name="pageNumber">1-based page number. Values below 1 are clamped to 1.</param>
    /// <param name="pageSize">Rows per page. Values below 1 are clamped to 1.</param>
    /// <param name="cancellationToken">Abandons the query when the caller goes away.</param>
    Task<PagedResult<RoomDto>> GetRoomsAsync(
        RoomFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
