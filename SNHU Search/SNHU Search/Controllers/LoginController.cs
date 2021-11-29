using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SNHU_Search.Models;
using Microsoft.AspNetCore.Http;

namespace SNHU_Search.Controllers
{
    public class LoginController : Controller
    {
        private readonly DBManager _manager;
        private string Cookiekey = "LoginUserName";
        public LoginController(DBManager manager)
        {
            _manager = manager;
        }

        public IActionResult Index()
        {
            return View("~/Views/Login/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel loginUser)
        {
            bool bUserExists = false;

            if (ModelState.IsValid)
            {
                int nUserID = _manager.GetUserID(loginUser.UserName);

                // Checks Password
                bUserExists = _manager.CheckPassword(loginUser, loginUser.UserName, nUserID);
                if (bUserExists)
                {
                    HttpContext.Session.SetString("userid", nUserID.ToString());
                    _manager.LoadUser(loginUser, ref nUserID, loginUser.UserName);
                                       
                    string key = "LoginUserName";
                    string value = loginUser.UserName;

                    CookieOptions options = new CookieOptions();
                    options.Expires = DateTime.Now.AddDays(7);
                    Response.Cookies.Append(key, value, options);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.message = "Username not found or password incorrect!";
                }
            }

            // Go to Login screen
            return View("Login");
        }

        public IActionResult SignUp()
        {
            return View();
        }       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(SignupModel SignUpM)
        {
            bool bRet = false;
            // Checks if all required fields are met
            if (ModelState.IsValid)
            {
                if (SignUpM.Password.Length < 4 && SignUpM.ConfirmPassword.Length < 4)
                    ViewBag.message = "Password must be greater than 4 characters!";
                else
                {
                    bRet = _manager.SaveUser(SignUpM);

                    if (!bRet)
                    {
                        ViewBag.message = "Account info saved successfully!";
                        // Go to Login page
                        return View("Login");
                    }
                    else
                    {
                        ViewBag.message = "Username taken, choose a different one!";
                    }
                }
            }
            return View("~/Views/Login/SignUp.cshtml");
        }
        public IActionResult Logout()
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Append(Cookiekey, "ExpireCookie", options);

            return View("~/Views/Home/Index.cshtml");
        }
    }
}