using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QwestRooms.DAL.Models;
using QwestRooms.UI.Models;

namespace QwestRooms.UI.Controllers;

/// <summary>
/// Registration, sign-in and sign-out over ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// The 2019 version resolved its user manager from the OWIN context inside the constructor, where
/// <c>HttpContext</c> is still null, so every request to this controller threw before reaching an
/// action. Here both managers are ordinary constructor-injected services.
/// </remarks>
[Authorize]
public sealed class AccountController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager) : Controller
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new AppUser { UserName = model.Login, Email = model.Login };
        var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);

        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(false);
            return RedirectToAction(nameof(RoomController.Index), "Room");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager
            .PasswordSignInAsync(model.Login, model.Password, model.RememberMe, lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked out. Try again in a few minutes.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            // Deliberately vague: saying which of the two was wrong would let a caller enumerate
            // registered accounts.
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync().ConfigureAwait(false);
        return RedirectToAction(nameof(RoomController.Index), "Room");
    }

    /// <summary>
    /// Only follows a return URL that is local to this application; anything else would make the
    /// sign-in form an open redirect.
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction(nameof(RoomController.Index), "Room");
}
