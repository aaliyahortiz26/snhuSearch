using System.ComponentModel.DataAnnotations;


namespace SNHU_Search.Models
{
    public class ForgetPasswordModel
    {
        [Required(ErrorMessage = "Enter Your Email. *")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        public string FirstName { get; set; } //might remove this after, do I need?
    }
}
