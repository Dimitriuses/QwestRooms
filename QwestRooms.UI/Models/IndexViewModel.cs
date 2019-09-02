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

        //public IEnumerable<AdressDTO> Adresses { get; set; }

        //public void getAdresesWithRooms()
        //{
        //    if(Rooms.Count() > 0)
        //    {
        //        HashSet<AdressDTO> adresses = new HashSet<AdressDTO>();
        //        foreach (RoomDTO item in Rooms)
        //        {
        //            adresses.Add(item.Adress);
        //        }
        //        Adresses = adresses;
        //    }
        //}
    }
}