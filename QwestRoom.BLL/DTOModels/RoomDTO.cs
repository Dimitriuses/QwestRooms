using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRoom.BLL.DTOModels
{
    public class RoomDTO
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

        public AdressDTO Adress { get; set; }
        public CompanyDTO Company { get; set; }
        public List<ImageDTO> Images { get; set; }

    }
}
