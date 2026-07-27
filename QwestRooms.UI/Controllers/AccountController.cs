using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using QwestRooms.DAL.Models;
using QwestRooms.UI.App_Start;
using QwestRooms.UI.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QwestRooms.UI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
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

        // No constructor: Controller.HttpContext is still null while the controller is being
        // constructed, so resolving the OWIN context here threw a NullReferenceException on
        // every request. Both properties above resolve it per request instead.

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(UserRegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new AppUser
            {
                Email = model.Login,
                UserName = model.Login
            };

            var result = await manager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Room");
            }

            AddErrors(result);
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLoginVM model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await manager.FindAsync(model.Login, model.Password);
            if (user == null)
            {
                // Deliberately vague: saying which of the two was wrong would let an attacker
                // enumerate registered accounts.
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            await SignInAsync(user, model.RememberMe);
            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Room");
        }

        private async Task SignInAsync(AppUser user, bool isPersistent)
        {
            AuthManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            var identity = await manager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        /// <summary>
        /// Only redirects to URLs local to this application. Redirecting to an arbitrary
        /// caller-supplied returnUrl would be an open redirect.
        /// </summary>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Room");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}
