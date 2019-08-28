using QwestRoom.BLL.DTOModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwestRoom.BLL.Services.Abstraction
{
    public interface IRoomsService
    {
        ICollection<RoomDTO> GetRooms();
    }
}
