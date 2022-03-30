using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SNHU_Search.Models
{
    public class ProfileModel
    {
        [DataType(DataType.Upload)]
        [Display(Name = "Upload File")]

        public IFormFile ProfileImage { get; set; }
        public string FileName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
