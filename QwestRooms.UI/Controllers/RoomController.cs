using QwestRoom.BLL.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QwestRooms.UI.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRoomsService roomsService;
        public RoomController(IRoomsService _roomsService)
        {
            roomsService = _roomsService;
        }
        // GET: Room
        public ActionResult Index()
        {
            
            return View(roomsService.GetRooms());
        }
    }
}