using QwestRoom.BLL.DTOModels;
using QwestRooms.DAL.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace QwestRoom.BLL.Mapping
{
    /// <summary>
    /// Entity-to-DTO projections, declared once and reused by the services.
    /// <para>
    /// These are <see cref="Expression"/>s rather than functions on purpose: applied through
    /// <c>Select</c>, the query provider turns them into the SELECT list of a single SQL
    /// statement. Only the mapped columns are read, and related rows come back in the same round
    /// trip -- so the room list no longer issues a query per room for its company, address and
    /// images.
    /// </para>
    /// <para>
    /// AutoMapper would be the conventional choice here, but every release compatible with
    /// .NET Framework 4.8 carries an unpatched high-severity advisory (GHSA-rvv3-g6hj-g44x, fixed
    /// only in 15.1.1, which requires .NET 8). Hand-written expressions keep the dependency count
    /// and the advisory count at zero.
    /// </para>
    /// </summary>
    public static class Projections
    {
        public static readonly Expression<Func<Country, CountryDTO>> ToCountryDTO =
            country => new CountryDTO
            {
                Id = country.Id,
                Name = country.Name
            };

        public static readonly Expression<Func<City, CityDTO>> ToCityDTO =
            city => new CityDTO
            {
                Id = city.Id,
                Name = city.Name
            };

        public static readonly Expression<Func<Address, AddressDTO>> ToAddressDTO =
            address => new AddressDTO
            {
                Id = address.Id,
                HouseNumber = address.HouseNumber,
                Country = new CountryDTO { Id = address.Country.Id, Name = address.Country.Name },
                City = new CityDTO { Id = address.City.Id, Name = address.City.Name },
                Street = new StreetDTO { Id = address.Street.Id, Name = address.Street.Name }
            };

        // The address initialiser is repeated rather than referencing ToAddressDTO: an expression
        // tree cannot invoke another expression and still be translatable to SQL by EF6. Composing
        // them would need a rewriter such as LINQKit, which is not worth a dependency here.
        public static readonly Expression<Func<Room, RoomDTO>> ToRoomDTO =
            room => new RoomDTO
            {
                Id = room.Id,
                Name = room.Name,
                Description = room.Description,
                TimeToPass = room.TimeToPass,
                MinPlayers = room.MinPlayers,
                MaxPlayers = room.MaxPlayers,
                Phone = room.Phone,
                Email = room.Email,
                Rating = room.Rating,
                FearLevel = room.FearLevel,
                Difficulty = room.Difficulty,
                LogoPath = room.LogoPath,
                Company = new CompanyDTO { Id = room.Company.Id, Name = room.Company.Name },
                Address = new AddressDTO
                {
                    Id = room.Address.Id,
                    HouseNumber = room.Address.HouseNumber,
                    Country = new CountryDTO { Id = room.Address.Country.Id, Name = room.Address.Country.Name },
                    City = new CityDTO { Id = room.Address.City.Id, Name = room.Address.City.Name },
                    Street = new StreetDTO { Id = room.Address.Street.Id, Name = room.Address.Street.Name }
                },
                Images = room.Images.Select(image => new ImageDTO
                {
                    Id = image.Id,
                    Path = image.Path
                }).ToList()
            };
    }
}
