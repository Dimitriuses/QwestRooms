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
            ViewBag.Country = "Не Вибрано";
            ViewData["CountriData"] = GetAllCountry();
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

        public /*ActionResult*/ List<CountryVievModel> GetAllCountry()
        {
            var listadresses = adressesService.GetAdresses();
            HashSet<CountryVievModel> Countries = new HashSet<CountryVievModel>();
            //Countries.Add(new CountryVievModel { Id = -1, Name = "Не вибрано" });
            foreach (var item in listadresses)
            {
                int Dublicate = 0;
                foreach (var item1 in Countries)
                {
                    if(item1.Id == item.Country.Id)
                    {
                        Dublicate++;
                    }
                }
                if (Dublicate == 0)
                {
                    Countries.Add(new CountryVievModel() {
                        Id = item.Country.Id,
                        Name = item.Country.Name
                    });
                }

            }

            return  /*PartialView("CountryColectionViev", */Countries.OrderBy(item => item.Name).ToList()/*)*/;
        }

        public ActionResult GetAllCitiesByCouyntries(int i)
        {
            var listadresses = adressesService.GetAdresses();
            List<CityViewModel> cities = new List<CityViewModel>();
            string country = "";
            foreach (var item in listadresses)
            {
                if(item.Country.Id == i)
                {
                    cities.Add(new CityViewModel { Id = item.City.Id, Name = item.City.Name });
                    country = item.Country.Name;
                }
            }
            ViewBag.Country = country;
            return PartialView("CitiesColectionView", cities.OrderBy(itrm => itrm.Name).ToList());
        }
    }
}