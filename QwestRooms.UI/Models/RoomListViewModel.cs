using QwestRooms.BLL.DTOModels;
using System.Collections.Generic;

namespace QwestRooms.UI.Models
{
    /// <summary>Model for the rooms grid partial: one page of rooms plus its pager state.</summary>
    public class RoomListViewModel
    {
        public IReadOnlyList<RoomDTO> Rooms { get; set; }

        public PageViewModel Page { get; set; }

        public RoomFilterViewModel Filter { get; set; }
    }
}
