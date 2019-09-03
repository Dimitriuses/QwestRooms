using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using QwestRooms.DAL.Models;
using QwestRooms.UI.App_Start;
using QwestRooms.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QwestRooms.UI.Controllers
{
    public class AcountController : Controller
    {
        //private readonly AppUserManager manager;
        //private readonly IAuthenticationManager AuthManager;
        private AppUserManager _userManager;



        public AppUserManager manager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public AcountController()
        {
            manager = HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            //AuthManager = HttpContext.GetOwinContext().Authentication;
        }
        // GET: Acount
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(UserRegisterVM model)
        {
            var user = new AppUser()
            {
                Email = model.Login,
                UserName = model.Login,

            };
            var res =  manager.Create(user, model.Password);
            if (res.Succeeded)
            {
                return RedirectToAction("Index","Room");
            }
            return View();
        }
    }
}