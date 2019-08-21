using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QwestRooms.UI.Controllers
{
    public class TestController : Controller
    {
        public readonly ICitiesService
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }
    }
}