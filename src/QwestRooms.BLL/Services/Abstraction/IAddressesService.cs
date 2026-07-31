using QwestRooms.BLL.Dtos;

namespace QwestRooms.BLL.Services.Abstraction;

/// <summary>
/// Supplies the options for the country -> city -> address filter. Each call returns only the
/// values reachable given the choices already made, so a user cannot assemble a combination that
/// matches nothing.
/// </summary>
public interface IAddressesService
{
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CityDto>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AddressDto>> GetAddressesByCountryAndCityAsync(
        int countryId,
        int cityId,
        CancellationToken cancellationToken = default);
}
