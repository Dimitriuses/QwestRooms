using QwestRooms.BLL.Dtos;
using QwestRooms.BLL.Filtering;
using QwestRooms.BLL.Mapping;
using QwestRooms.BLL.Services.Abstraction;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;

namespace QwestRooms.BLL.Services.Implementation;

/// <summary>
/// Reads pages of the room catalogue.
/// </summary>
/// <remarks>
/// Every method here composes one <see cref="IQueryable{T}"/> and executes it exactly twice: once
/// to count the matches for the pager, once to fetch the page. Nothing is filtered, sorted or
/// paged in memory. <c>QueryCountTests</c> pins that with a command counter, because it is the
/// kind of property that a single innocent-looking edit silently reverses.
/// </remarks>
public sealed class RoomsService(IGenericRepository<Room> roomRepository) : IRoomsService
{
    private readonly IGenericRepository<Room> _roomRepository = roomRepository;

    public async Task<PagedResult<RoomDto>> GetRoomsAsync(
        RoomFilter filter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);

        var query = ApplyFilter(_roomRepository.Query(), filter);

        // Counted by the database. The 2019 controller materialised every row and called .Count
        // on the resulting list.
        var totalCount = await _roomRepository.CountAsync(query, cancellationToken).ConfigureAwait(false);

        // Skip needs a deterministic order or the same row can appear on two pages, so order first.
        var page = query
            .OrderBy(room => room.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(Projections.ToRoomDto);

        var items = await _roomRepository.ToListAsync(page, cancellationToken).ConfigureAwait(false);

        return new PagedResult<RoomDto>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// A specific address is the narrowest choice, so it wins outright; otherwise country and city
    /// narrow independently. This is the same intent as the three near-identical branches of
    /// nested foreach loops that used to sit in the controller, expressed once and run as SQL.
    /// </summary>
    private static IQueryable<Room> ApplyFilter(IQueryable<Room> query, RoomFilter? filter)
    {
        if (filter is null)
        {
            return query;
        }

        if (filter.AddressId is { } addressId)
        {
            return query.Where(room => room.AddressId == addressId);
        }

        if (filter.CountryId is { } countryId)
        {
            query = query.Where(room => room.Address.CountryId == countryId);
        }

        if (filter.CityId is { } cityId)
        {
            query = query.Where(room => room.Address.CityId == cityId);
        }

        return query;
    }
}
