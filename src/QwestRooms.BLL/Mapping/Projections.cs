using System.Linq.Expressions;
using QwestRooms.BLL.Dtos;
using QwestRooms.DAL.Models;

namespace QwestRooms.BLL.Mapping;

/// <summary>
/// Entity-to-DTO projections, declared once and reused by the services.
/// </summary>
/// <remarks>
/// <para>
/// These are <see cref="Expression"/>s rather than functions on purpose. Applied through
/// <c>Select</c>, the query provider turns them into the SELECT list of a single statement: only
/// the mapped columns are read, and the related rows arrive in the same round trip. That is the
/// entire fix for the room list, which used to issue one query per room for its company, address
/// and images.
/// </para>
/// <para>
/// AutoMapper is the conventional choice for this job and would collapse the repetition below, but
/// it earns its keep on large, changing models; here it would add a dependency, a startup profile
/// scan and a layer of indirection in exchange for removing about forty lines that the compiler
/// checks for us.
/// </para>
/// </remarks>
public static class Projections
{
    public static Expression<Func<Country, CountryDto>> ToCountryDto { get; } =
        country => new CountryDto
        {
            Id = country.Id,
            Name = country.Name
        };

    public static Expression<Func<City, CityDto>> ToCityDto { get; } =
        city => new CityDto
        {
            Id = city.Id,
            Name = city.Name
        };

    public static Expression<Func<Address, AddressDto>> ToAddressDto { get; } =
        address => new AddressDto
        {
            Id = address.Id,
            HouseNumber = address.HouseNumber,
            Country = new CountryDto { Id = address.Country.Id, Name = address.Country.Name },
            City = new CityDto { Id = address.City.Id, Name = address.City.Name },
            Street = new StreetDto { Id = address.Street.Id, Name = address.Street.Name }
        };

    // The address initialiser is spelled out again rather than referencing ToAddressDto: an
    // expression tree cannot invoke another expression and stay translatable to SQL. Composing the
    // two would need a rewriter such as LINQKit, which is not worth a dependency for one call site.
    public static Expression<Func<Room, RoomDto>> ToRoomDto { get; } =
        room => new RoomDto
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
            Company = new CompanyDto { Id = room.Company.Id, Name = room.Company.Name },
            Address = new AddressDto
            {
                Id = room.Address.Id,
                HouseNumber = room.Address.HouseNumber,
                Country = new CountryDto { Id = room.Address.Country.Id, Name = room.Address.Country.Name },
                City = new CityDto { Id = room.Address.City.Id, Name = room.Address.City.Name },
                Street = new StreetDto { Id = room.Address.Street.Id, Name = room.Address.Street.Name }
            },
            Images = room.Images.Select(image => new ImageDto
            {
                Id = image.Id,
                Path = image.Path
            }).ToList()
        };
}
