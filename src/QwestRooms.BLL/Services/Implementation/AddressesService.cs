using QwestRooms.BLL.Dtos;
using QwestRooms.BLL.Mapping;
using QwestRooms.BLL.Services.Abstraction;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;

namespace QwestRooms.BLL.Services.Implementation;

public sealed class AddressesService(IGenericRepository<Address> addressRepository) : IAddressesService
{
    private readonly IGenericRepository<Address> _addressRepository = addressRepository;

    /// <summary>
    /// The distinct countries that have at least one address.
    /// </summary>
    /// <remarks>
    /// The 2019 implementation loaded every address, then de-duplicated the countries with a
    /// nested loop over a growing <c>HashSet</c> whose element type had no equality of its own --
    /// quadratic, and in memory. This is one <c>SELECT DISTINCT</c>.
    /// </remarks>
    public Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        var query = _addressRepository.Query()
            .Select(address => address.Country)
            .Distinct()
            .OrderBy(country => country.Name)
            .Select(Projections.ToCountryDto);

        return ExecuteAsync(query, cancellationToken);
    }

    public Task<IReadOnlyList<CityDto>> GetCitiesByCountryAsync(
        int countryId,
        CancellationToken cancellationToken = default)
    {
        // Distinct is load-bearing: the old version appended one entry per matching address, so a
        // country with several addresses in one city offered that city several times.
        var query = _addressRepository.Query()
            .Where(address => address.CountryId == countryId)
            .Select(address => address.City)
            .Distinct()
            .OrderBy(city => city.Name)
            .Select(Projections.ToCityDto);

        return ExecuteAsync(query, cancellationToken);
    }

    public Task<IReadOnlyList<AddressDto>> GetAddressesByCountryAndCityAsync(
        int countryId,
        int cityId,
        CancellationToken cancellationToken = default)
    {
        var query = _addressRepository.Query()
            .Where(address => address.CountryId == countryId && address.CityId == cityId)
            .OrderBy(address => address.Street.Name)
            .ThenBy(address => address.HouseNumber)
            .Select(Projections.ToAddressDto);

        return ExecuteAsync(query, cancellationToken);
    }

    private async Task<IReadOnlyList<T>> ExecuteAsync<T>(IQueryable<T> query, CancellationToken cancellationToken) =>
        await _addressRepository.ToListAsync(query, cancellationToken).ConfigureAwait(false);
}
