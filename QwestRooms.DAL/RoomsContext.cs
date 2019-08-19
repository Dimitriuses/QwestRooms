namespace QwestRooms.DAL
{
    using QwestRooms.DAL.Models;
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class RoomsContext : DbContext
    {
        public RoomsContext()
            : base("name=RoomsContext")
        {
        }

        public DbSet<Room> Rooms { get; set; }

    }

}