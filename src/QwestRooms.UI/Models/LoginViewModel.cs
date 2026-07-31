using System.ComponentModel.DataAnnotations;

namespace QwestRooms.UI.Models;

public sealed class LoginViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string Login { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
