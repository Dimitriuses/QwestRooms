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
        private readonly IAdressesService adressesService;
        public RoomController(IRoomsService _roomsService, IAdressesService _adressesService )
        {
            roomsService = _roomsService;
            adressesService = _adressesService;
        }
        // GET: Room
        public ActionResult Index(int page = 1)
        {
            int pageSize = 27;
            var listrooms = roomsService.GetRooms();
            var listadresses = adressesService.GetAdresses();
            PageViewModel pageViewModel = new PageViewModel(listrooms.Count, page, pageSize);
            IndexViewModel viewModel = new IndexViewModel
            {
                PageViewModel = pageViewModel,
                Rooms = listrooms.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                Adresses = listadresses
            };
            //viewModel.getAdresesWithRooms();
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

        public ActionResult GetAllCountry()
        {
            var listadresses = adressesService.GetAdresses();
            HashSet<CountryVievModel> Countries = new HashSet<CountryVievModel>();

            foreach (var item in listadresses)
            {
                Countries.Add(new CountryVievModel() {
                    Id = item.Country.Id,
                    Name = item.Country.Name
                });
            }
            return PartialView("CountryColectionViev", Countries.OrderBy(item => item.Name).ToList());
        }

        //public ActionResult GetAllCitiesByCouyntries(int i)
        //{

        //}
    }
}