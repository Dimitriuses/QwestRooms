using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRooms.DAL.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; }
        virtual public ICollection<Room> Rooms { get; set; }
    }
}
