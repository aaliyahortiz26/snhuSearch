using System.ComponentModel.DataAnnotations;

namespace SNHU_Search.Models
{
    public class SignupModel
    {
        [StringLength(64, MinimumLength = 1)]
        public string FirstName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        public string LastName { get; set; }

        [StringLength(64, MinimumLength = 1)]
        public string UserName { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string Email { get; set; }

        [StringLength(14, MinimumLength = 4)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Passwords do not match!")]
        [StringLength(14, MinimumLength = 4)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }
}
