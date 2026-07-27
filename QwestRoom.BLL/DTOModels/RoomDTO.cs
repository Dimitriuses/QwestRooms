using System;
using System.Collections.Generic;

namespace QwestRoom.BLL.DTOModels
{
    public class RoomDTO
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

        public AddressDTO Address { get; set; }
        public CompanyDTO Company { get; set; }
        public List<ImageDTO> Images { get; set; }
    }
}
