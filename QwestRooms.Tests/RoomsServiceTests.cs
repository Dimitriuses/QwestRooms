using Moq;
using QwestRooms.BLL.Filtering;
using QwestRooms.BLL.Services.Implementation;
using QwestRooms.DAL.Models;
using QwestRooms.DAL.Repositories;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace QwestRooms.Tests
{
    /// <summary>
    /// Covers the filtering that used to live in the controller as three near-identical branches
    /// of nested foreach loops. Running the real projection expression over an in-memory
    /// IQueryable exercises the mapping as well as the filter.
    /// </summary>
    public class RoomsServiceTests
    {
        private static readonly Country Ukraine = TestData.Country(1, "Ukraine");
        private static readonly Country Poland = TestData.Country(2, "Poland");
        private static readonly City Kyiv = TestData.City(10, "Kyiv");
        private static readonly City Lviv = TestData.City(11, "Lviv");
        private static readonly City Warsaw = TestData.City(12, "Warsaw");
        private static readonly Street Main = TestData.Street(100, "Main");

        private static readonly Address KyivA = TestData.Address(1000, Ukraine, Kyiv, Main, "1");
        private static readonly Address KyivB = TestData.Address(1001, Ukraine, Kyiv, Main, "2");
        private static readonly Address LvivA = TestData.Address(1002, Ukraine, Lviv, Main, "3");
        private static readonly Address WarsawA = TestData.Address(1003, Poland, Warsaw, Main, "4");

        private static RoomsService CreateService(params Room[] rooms)
        {
            var repository = new Mock<IGenericRepository<Room>>();
            repository.Setup(r => r.GetAll()).Returns(rooms.AsQueryable());
            return new RoomsService(repository.Object);
        }

        private static Room[] SampleRooms()
        {
            return new[]
            {
                TestData.Room(1, KyivA),
                TestData.Room(2, KyivB),
                TestData.Room(3, LvivA),
                TestData.Room(4, WarsawA)
            };
        }

        [Fact]
        public void GetRooms_WithoutFilter_ReturnsEverything()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(RoomFilter.None, 1, 10);

            Assert.Equal(4, result.TotalCount);
            Assert.Equal(4, result.Items.Count);
        }

        [Fact]
        public void GetRooms_WithNullFilter_ReturnsEverything()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(null, 1, 10);

            Assert.Equal(4, result.TotalCount);
        }

        [Fact]
        public void GetRooms_FiltersByCountry()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(new RoomFilter { CountryId = Ukraine.Id }, 1, 10);

            Assert.Equal(3, result.TotalCount);
            Assert.All(result.Items, room => Assert.Equal("Ukraine", room.Address.Country.Name));
        }

        [Fact]
        public void GetRooms_FiltersByCountryAndCity()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(
                new RoomFilter { CountryId = Ukraine.Id, CityId = Kyiv.Id }, 1, 10);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, room => Assert.Equal("Kyiv", room.Address.City.Name));
        }

        [Fact]
        public void GetRooms_AddressId_TakesPrecedenceOverCountryAndCity()
        {
            var service = CreateService(SampleRooms());

            // A deliberately contradictory filter: the address is in Poland, the country/city say
            // Ukraine/Kyiv. The narrowest criterion is meant to win outright.
            var result = service.GetRooms(
                new RoomFilter { CountryId = Ukraine.Id, CityId = Kyiv.Id, AddressId = WarsawA.Id },
                1,
                10);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(WarsawA.Id, result.Items.Single().Address.Id);
        }

        [Fact]
        public void GetRooms_TotalCount_CountsAllMatches_NotJustThePage()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(RoomFilter.None, 1, 2);

            Assert.Equal(4, result.TotalCount); // all matches
            Assert.Equal(2, result.Items.Count); // one page of them
        }

        [Fact]
        public void GetRooms_SecondPage_ReturnsTheNextSlice()
        {
            var service = CreateService(SampleRooms());

            var first = service.GetRooms(RoomFilter.None, 1, 2);
            var second = service.GetRooms(RoomFilter.None, 2, 2);

            Assert.Equal(new[] { 1, 2 }, first.Items.Select(r => r.Id).ToArray());
            Assert.Equal(new[] { 3, 4 }, second.Items.Select(r => r.Id).ToArray());
        }

        [Fact]
        public void GetRooms_PageBeyondTheEnd_ReturnsEmptyButKeepsTotalCount()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(RoomFilter.None, 99, 2);

            Assert.Empty(result.Items);
            Assert.Equal(4, result.TotalCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void GetRooms_ClampsPageNumberToOne(int requestedPage)
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(RoomFilter.None, requestedPage, 2);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(new[] { 1, 2 }, result.Items.Select(r => r.Id).ToArray());
        }

        [Fact]
        public void GetRooms_ClampsPageSizeToOne()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(RoomFilter.None, 1, 0);

            Assert.Equal(1, result.PageSize);
            Assert.Single(result.Items);
        }

        [Fact]
        public void GetRooms_ProjectsNestedPropertiesAndRenamedColumns()
        {
            var room = TestData.Room(1, KyivA);
            room.Images = new List<Image> { new Image { Id = 7, Path = "/a.png" } };
            var service = CreateService(room);

            var dto = service.GetRooms(RoomFilter.None, 1, 10).Items.Single();

            Assert.Equal(2, dto.MinPlayers);
            Assert.Equal(4, dto.Difficulty);
            Assert.Equal("Test Company", dto.Company.Name);
            Assert.Equal("Kyiv", dto.Address.City.Name);
            Assert.Equal("Ukraine", dto.Address.Country.Name);
            Assert.Equal("Main", dto.Address.Street.Name);
            Assert.Equal("/a.png", Assert.Single(dto.Images).Path);
        }

        [Fact]
        public void GetRooms_ReturnsEmpty_WhenNothingMatches()
        {
            var service = CreateService(SampleRooms());

            var result = service.GetRooms(new RoomFilter { CountryId = 9999 }, 1, 10);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }
    }
}
