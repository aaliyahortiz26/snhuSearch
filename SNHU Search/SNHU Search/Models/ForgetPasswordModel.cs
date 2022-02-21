using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddress = new List<ForgetPasswordModel>();
            FromAddress = new List<ForgetPasswordModel>();
        }
        public List<ForgetPasswordModel> ToAddress { get; set; }
        public List<ForgetPasswordModel> FromAddress { get; set; }
        public string EmailSubject { get; set; }
        public string EmailContent { get; set; }

    }
}
