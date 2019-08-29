using QwestRoom.BLL.DTOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QwestRooms.UI.Models
{
    public class IndexViewModel
    {
        public IEnumerable<RoomDTO> Rooms { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}