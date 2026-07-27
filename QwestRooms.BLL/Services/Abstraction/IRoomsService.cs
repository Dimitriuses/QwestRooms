using QwestRooms.BLL.DTOModels;
using QwestRooms.BLL.Filtering;

namespace QwestRooms.BLL.Services.Abstraction
{
    public interface IRoomsService
    {
        /// <summary>
        /// Returns a single page of rooms matching <paramref name="filter"/>, together with the
        /// total number of matches. Filtering and paging are applied in the database, not in
        /// memory.
        /// </summary>
        PagedResult<RoomDTO> GetRooms(RoomFilter filter, int pageNumber, int pageSize);
    }
}
