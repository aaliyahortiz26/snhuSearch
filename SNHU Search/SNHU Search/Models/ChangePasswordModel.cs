using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace SNHU_Search.Models
{
    public class ChangePasswordModel
    {
        public string userOldPassword { get; set; }

        [StringLength(14, MinimumLength = 4)]
        public string userNewPassword { get; set; }

        [Required(ErrorMessage = "Passwords do not match!")]
        [StringLength(14, MinimumLength = 4)]
        [Compare("userNewPassword")]
        public string userConfirmNewPassword { get; set; }
    }
}
