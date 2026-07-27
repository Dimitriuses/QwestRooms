using QwestRooms.BLL.DTOModels;
using System.Collections.Generic;

namespace QwestRooms.BLL.Services.Abstraction
{
    /// <summary>
    /// Supplies the options for the country -> city -> address filter. Each call returns only the
    /// values that are actually reachable given the choices already made, so the user cannot pick
    /// a combination that matches nothing.
    /// </summary>
    public interface IAddressesService
    {
        IReadOnlyList<CountryDTO> GetCountries();

        IReadOnlyList<CityDTO> GetCitiesByCountry(int countryId);

        IReadOnlyList<AddressDTO> GetAddressesByCountryAndCity(int countryId, int cityId);
    }
}
