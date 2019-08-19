using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRooms.DAL.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Path { get; set; }

        virtual public Room Room { get; set; }
    }
}
