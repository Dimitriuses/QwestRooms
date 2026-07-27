using QwestRooms.DAL.Models;
using System;
using System.Collections.Generic;

namespace QwestRooms.Tests
{
    /// <summary>
    /// Builds small in-memory object graphs for the service tests.
    /// <para>
    /// Related entities are deliberately shared instances rather than copies, because that is
    /// what EF's identity map produces at runtime -- and it is what makes LINQ-to-Objects
    /// <c>Distinct()</c> behave like the SQL <c>SELECT DISTINCT</c> the services actually run.
    /// </para>
    /// </summary>
    internal static class TestData
    {
        public static Country Country(int id, string name)
        {
            return new Country { Id = id, Name = name };
        }

        public static City City(int id, string name)
        {
            return new City { Id = id, Name = name };
        }

        public static Street Street(int id, string name)
        {
            return new Street { Id = id, Name = name };
        }

        public static Address Address(int id, Country country, City city, Street street, string houseNumber = "1")
        {
            return new Address
            {
                Id = id,
                HouseNumber = houseNumber,
                Country = country,
                City = city,
                Street = street
            };
        }

        public static Room Room(int id, Address address, string name = null, Company company = null)
        {
            return new Room
            {
                Id = id,
                Name = name ?? ("Room " + id),
                Description = "Description " + id,
                TimeToPass = TimeSpan.FromMinutes(60),
                MinPlayers = 2,
                MaxPlayers = 6,
                Phone = "555-000" + id,
                Email = "room" + id + "@example.com",
                Rating = 5,
                FearLevel = 3,
                Difficulty = 4,
                LogoPath = "/logo" + id + ".png",
                Address = address,
                Company = company ?? new Company { Id = 1, Name = "Test Company" },
                Images = new List<Image>()
            };
        }
    }
}
