using Microsoft.AspNet.Identity.EntityFramework;
using QwestRooms.DAL.Configuration;
using QwestRooms.DAL.Models;
using System.Data.Entity;

namespace QwestRooms.DAL
{
    public class RoomsContext : IdentityDbContext<AppUser>
    {
        // Registered in a static constructor so it runs once per AppDomain, rather than on every
        // context instantiation as it would in the instance constructor.
        static RoomsContext()
        {
            Database.SetInitializer(new RoomsDbInitializer());
        }

        public RoomsContext()
            : base("name=RoomsContext")
        {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Image> Images { get; set; }

        public static RoomsContext Create()
        {
            return new RoomsContext();
        }
    }
}
