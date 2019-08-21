using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRooms.DAL.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan TimeToPass { get; set; }
        public int MinPayers { get; set; }
        public int MaxPlayers { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public int FearLevel { get; set; }
        public int Diffictly { get; set; }
        public string LogoPath { get; set; }

        virtual public Adress Adress { get; set; }
        virtual public Company Company { get; set; }

        virtual public ICollection<Image> Images { get; set; }
    }
}
