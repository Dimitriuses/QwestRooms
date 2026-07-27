using System;
using System.Collections.Generic;

namespace QwestRooms.DAL.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan TimeToPass { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public int FearLevel { get; set; }
        public int Difficulty { get; set; }
        public string LogoPath { get; set; }

        public virtual Address Address { get; set; }
        public virtual Company Company { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
}
