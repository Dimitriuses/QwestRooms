using QwestRoom.BLL.Services.Abstraction;
using QwestRoom.BLL.Services.Implementation;
using QwestRooms.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QwestRooms.UI.Controllers
{
    public class TestController : Controller
    {
        private readonly ICitiesService citiesService;
        public TestController(ICitiesService _citiesService)
        {
            citiesService = _citiesService;
        }
        public ActionResult Index()
        {
            var cities = citiesService.GetCities().Select(cty => new CityViewModel { Id = cty.Id, Name = cty.Name }).ToList<CityViewModel>();
            return View("View",cities);
        }
    }
}