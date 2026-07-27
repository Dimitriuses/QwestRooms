using System.Collections.Generic;

namespace QwestRooms.DAL.Models
{
    public class Street
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
    }
}
