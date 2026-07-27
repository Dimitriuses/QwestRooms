using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using QwestRooms.DAL;
using QwestRooms.UI.App_Start;

[assembly: OwinStartup(typeof(QwestRooms.UI.Startup))]

namespace QwestRooms.UI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext<RoomsContext>(RoomsContext.Create);
            app.CreatePerOwinContext<AppUserManager>(AppUserManager.Create);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                // Where an unauthenticated user gets sent. Note the controller really is spelled
                // "Acount" -- correcting that typo is Phase 3.7, and this path has to match it
                // until then.
                LoginPath = new PathString("/Acount/Login")
            });
        }

    }
}
