using QwestRoom.BLL.DTOModels;
using QwestRoom.BLL.Mapping;
using QwestRoom.BLL.Services.Abstraction;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace QwestRoom.BLL.Services.Implementation
{
    public class AddressesService : IAddressesService
    {
        private readonly IGenericRepository<Address> addressRepository;

        public AddressesService(IGenericRepository<Address> _addressRepository)
        {
            addressRepository = _addressRepository;
        }

        /// <summary>
        /// Distinct countries that have at least one address.
        /// <para>
        /// The previous implementation loaded every address, then de-duplicated countries with a
        /// nested loop over a growing HashSet -- quadratic, and in memory. This is a SELECT
        /// DISTINCT.
        /// </para>
        /// </summary>
        public IReadOnlyList<CountryDTO> GetCountries()
        {
            return addressRepository.GetAll()
                .Select(address => address.Country)
                .Distinct()
                .OrderBy(country => country.Name)
                .Select(Projections.ToCountryDTO)
                .ToList();
        }

        public IReadOnlyList<CityDTO> GetCitiesByCountry(int countryId)
        {
            // Distinct matters here: the old version added one entry per matching address, so a
            // country with several addresses in the same city repeated that city in the dropdown.
            return addressRepository.GetAll()
                .Where(address => address.Country.Id == countryId)
                .Select(address => address.City)
                .Distinct()
                .OrderBy(city => city.Name)
                .Select(Projections.ToCityDTO)
                .ToList();
        }

        public IReadOnlyList<AddressDTO> GetAddressesByCountryAndCity(int countryId, int cityId)
        {
            return addressRepository.GetAll()
                .Where(address => address.Country.Id == countryId && address.City.Id == cityId)
                .OrderBy(address => address.Street.Name)
                .ThenBy(address => address.HouseNumber)
                .Select(Projections.ToAddressDTO)
                .ToList();
        }
    }
}
