using System.Collections.Generic;

namespace QwestRooms.DAL.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; }

        public virtual City City { get; set; }
        public virtual Country Country { get; set; }
        public virtual Street Street { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
