namespace QwestRooms.DAL
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using QwestRooms.DAL.Configuration;
    using QwestRooms.DAL.Models;
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class RoomsContext : IdentityDbContext<AppUser>
    {
        public RoomsContext()
            : base("name=RoomsContext")
        {
            //Database.SetInitializer(new DBinicialaiser());
        }

        public DbSet<Adress> Adresses { get; set; }
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