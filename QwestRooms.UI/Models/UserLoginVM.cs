using System.ComponentModel.DataAnnotations;

namespace QwestRooms.UI.Models
{
    public class UserLoginVM
    {
        [Required]
        [Display(Name = "Email")]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
