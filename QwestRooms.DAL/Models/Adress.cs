using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRooms.DAL.Models
{
    public class Adress
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; }
        virtual public City City { get; set; }
        virtual public Country Country { get; set; }
        virtual public Street Street { get; set; }
        virtual public ICollection<Room> Rooms { get; set; }

    }
}
