using System.ComponentModel.DataAnnotations;

namespace QwestRooms.UI.Models;

public sealed class RegisterViewModel
{
    // The same value becomes both the user name and the email address, and Identity is configured
    // with RequireUniqueEmail, so it has to be a valid address.
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Login { get; set; } = string.Empty;

    // Mirrors the password options set in Program.cs: at least 6 characters, with an upper case
    // letter, a lower case letter, a digit and a symbol.
    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
