using QwestRoom.BLL.Services.Abstraction;
using QwestRooms.UI.Models;
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
        public ActionResult Index(int page = 1)
        {
            int pageSize = 27;
            var listrooms = roomsService.GetRooms();
            PageViewModel pageViewModel = new PageViewModel(listrooms.Count, page, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Rooms = listrooms.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            };

            return View("Page",viewModel);
        }

        public ActionResult GetRoomsByPage(int page = 1)
        {
            int pageSize = 27;
            var listrooms = roomsService.GetRooms();
            //PageViewModel pageViewModel = new PageViewModel(listrooms.Count, page, pageSize);
            //IndexViewModel viewModel = new IndexViewModel
            //{
            //    PageViewModel = pageViewModel,
            //    Rooms = listrooms.Skip((page - 1) * pageSize).Take(pageSize).ToList()
            //};

            return PartialView("RoomsCollectionView", listrooms.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }
    }
}