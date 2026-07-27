using QwestRooms.BLL.DTOModels;
using QwestRooms.BLL.Filtering;
using QwestRooms.BLL.Mapping;
using QwestRooms.BLL.Services.Abstraction;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;
using System.Linq;

namespace QwestRooms.BLL.Services.Implementation
{
    public class RoomsService : IRoomsService
    {
        private readonly IGenericRepository<Room> roomRepository;

        public RoomsService(IGenericRepository<Room> _roomRepository)
        {
            roomRepository = _roomRepository;
        }

        public PagedResult<RoomDTO> GetRooms(RoomFilter filter, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 1;
            }

            var query = ApplyFilter(roomRepository.GetAll(), filter);

            // Counted in the database. The old controller pulled every room into memory and
            // called .Count on the resulting list.
            var totalCount = query.Count();

            // Skip requires a deterministic order, so order before paging.
            var items = query
                .OrderBy(room => room.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(Projections.ToRoomDTO)
                .ToList();

            return new PagedResult<RoomDTO>(items, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// A specific address is the narrowest choice, so it wins outright; otherwise country and
        /// city narrow independently. This is the same intent as the previous three near-identical
        /// nested-loop branches in the controller, expressed once and evaluated in SQL.
        /// </summary>
        private static IQueryable<Room> ApplyFilter(IQueryable<Room> query, RoomFilter filter)
        {
            if (filter == null)
            {
                return query;
            }

            if (filter.AddressId.HasValue)
            {
                return query.Where(room => room.Address.Id == filter.AddressId.Value);
            }

            if (filter.CountryId.HasValue)
            {
                query = query.Where(room => room.Address.Country.Id == filter.CountryId.Value);
            }

            if (filter.CityId.HasValue)
            {
                query = query.Where(room => room.Address.City.Id == filter.CityId.Value);
            }

            return query;
        }
    }
}
