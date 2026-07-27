using Moq;
using QwestRooms.BLL.Services.Implementation;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;
using System.Linq;
using Xunit;

namespace QwestRooms.Tests
{
    /// <summary>
    /// Covers the filter-option queries. The de-duplication assertions are the point: the
    /// original code de-duplicated countries with a quadratic HashSet loop, and did not
    /// de-duplicate cities at all -- so a country with several addresses in one city showed that
    /// city repeatedly in the dropdown.
    /// </summary>
    public class AddressesServiceTests
    {
        private static readonly Country Ukraine = TestData.Country(1, "Ukraine");
        private static readonly Country Poland = TestData.Country(2, "Poland");
        private static readonly City Kyiv = TestData.City(10, "Kyiv");
        private static readonly City Lviv = TestData.City(11, "Lviv");
        private static readonly City Warsaw = TestData.City(12, "Warsaw");
        private static readonly Street Main = TestData.Street(100, "Main");
        private static readonly Street Oak = TestData.Street(101, "Oak");

        private static AddressesService CreateService(params Address[] addresses)
        {
            var repository = new Mock<IGenericRepository<Address>>();
            repository.Setup(r => r.GetAll()).Returns(addresses.AsQueryable());
            return new AddressesService(repository.Object);
        }

        private static Address[] SampleAddresses()
        {
            return new[]
            {
                TestData.Address(1, Ukraine, Kyiv, Main, "1"),
                TestData.Address(2, Ukraine, Kyiv, Oak, "2"),   // same country + city as above
                TestData.Address(3, Ukraine, Lviv, Main, "3"),
                TestData.Address(4, Poland, Warsaw, Main, "4")
            };
        }

        [Fact]
        public void GetCountries_ReturnsEachCountryOnce()
        {
            var service = CreateService(SampleAddresses());

            var countries = service.GetCountries();

            Assert.Equal(2, countries.Count);
            Assert.Equal(new[] { "Poland", "Ukraine" }, countries.Select(c => c.Name).ToArray());
        }

        [Fact]
        public void GetCountries_IsOrderedByName()
        {
            var service = CreateService(SampleAddresses());

            var names = service.GetCountries().Select(c => c.Name).ToArray();

            Assert.Equal(names.OrderBy(n => n).ToArray(), names);
        }

        [Fact]
        public void GetCitiesByCountry_ReturnsEachCityOnce()
        {
            var service = CreateService(SampleAddresses());

            // Kyiv appears in two of Ukraine's addresses but must be offered once.
            var cities = service.GetCitiesByCountry(Ukraine.Id);

            Assert.Equal(2, cities.Count);
            Assert.Equal(new[] { "Kyiv", "Lviv" }, cities.Select(c => c.Name).ToArray());
        }

        [Fact]
        public void GetCitiesByCountry_ExcludesOtherCountries()
        {
            var service = CreateService(SampleAddresses());

            var cities = service.GetCitiesByCountry(Poland.Id);

            Assert.Equal("Warsaw", Assert.Single(cities).Name);
        }

        [Fact]
        public void GetCitiesByCountry_ReturnsEmpty_ForUnknownCountry()
        {
            var service = CreateService(SampleAddresses());

            Assert.Empty(service.GetCitiesByCountry(9999));
        }

        [Fact]
        public void GetAddressesByCountryAndCity_ReturnsOnlyMatches()
        {
            var service = CreateService(SampleAddresses());

            var addresses = service.GetAddressesByCountryAndCity(Ukraine.Id, Kyiv.Id);

            Assert.Equal(2, addresses.Count);
            Assert.All(addresses, a => Assert.Equal("Kyiv", a.City.Name));
        }

        [Fact]
        public void GetAddressesByCountryAndCity_IsOrderedByStreetThenHouseNumber()
        {
            var service = CreateService(SampleAddresses());

            var addresses = service.GetAddressesByCountryAndCity(Ukraine.Id, Kyiv.Id);

            Assert.Equal(new[] { "Main", "Oak" }, addresses.Select(a => a.Street.Name).ToArray());
        }

        [Fact]
        public void GetAddressesByCountryAndCity_ProjectsTheWholeAddress()
        {
            var service = CreateService(SampleAddresses());

            var address = service.GetAddressesByCountryAndCity(Poland.Id, Warsaw.Id).Single();

            Assert.Equal("4", address.HouseNumber);
            Assert.Equal("Warsaw", address.City.Name);
            Assert.Equal("Poland", address.Country.Name);
            Assert.Equal("Main", address.Street.Name);
        }
    }
}
