using Microsoft.AspNetCore.Identity;

namespace QwestRooms.DAL.Models;

/// <summary>
/// The application's user. It adds nothing to <see cref="IdentityUser"/> yet, but naming it here
/// means profile fields can be added later without touching every Identity generic argument.
/// </summary>
public class AppUser : IdentityUser
{
}
