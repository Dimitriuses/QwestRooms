using QwestRoom.BLL.DTOModels;
using System.Collections.Generic;

namespace QwestRooms.UI.Models
{
    /// <summary>Model for the full catalogue page: the rooms grid plus the filter's options.</summary>
    public class RoomCatalogViewModel
    {
        public RoomListViewModel List { get; set; }

        public IReadOnlyList<CountryDTO> Countries { get; set; }
    }
}
