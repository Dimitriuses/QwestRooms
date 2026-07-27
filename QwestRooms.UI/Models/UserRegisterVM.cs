using System.ComponentModel.DataAnnotations;

namespace QwestRooms.UI.Models
{
    public class UserRegisterVM
    {
        // The same value is used as both the user name and the email address, and
        // AppUserManager is configured with RequireUniqueEmail, so it has to be a valid address.
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Login { get; set; }

        // Mirrors the PasswordValidator configured in AppUserManager.Create: at least 6
        // characters, with an upper case letter, a lower case letter, a digit and a symbol.
        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
