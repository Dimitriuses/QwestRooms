using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using QwestRooms.DAL;
using QwestRooms.DAL.Models;
using System;

namespace QwestRooms.UI.App_Start
{
    public class AppUserManager : UserManager<AppUser>
    {
        public AppUserManager(IUserStore<AppUser> store)
           : base(store)
        {
        }

        public static AppUserManager Create(IdentityFactoryOptions<AppUserManager> options, IOwinContext context)
        {
            var manager = new AppUserManager(new UserStore<AppUser>(context.Get<RoomsContext>()));

            manager.UserValidator = new UserValidator<AppUser>(manager)
            {
                // User names are email addresses here, so they are not alphanumeric-only.
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // UserRegisterVM's annotations mirror these rules, so client-side validation and the
            // server agree. Change one and the other needs the same change.
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
            };

            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<AppUser>(dataProtectionProvider.Create("QwestRooms"));
            }

            return manager;
        }
    }
}
