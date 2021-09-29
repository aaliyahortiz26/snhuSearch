using System.ComponentModel.DataAnnotations;

namespace SNHU_Search.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Enter Your UserName. *")]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Enter Your Password. *")]
        [DataType(DataType.Password)]
        [Display(Name = "Pass")]
        public string Password { get; set; }
    }
}