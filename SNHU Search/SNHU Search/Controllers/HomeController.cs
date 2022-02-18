using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SNHU_Search.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SNHU_Search.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBManager _manager;
        private readonly ElasticManager _ManagerElastic = new ElasticManager();
        private readonly PythonModel pythonScraper = new PythonModel();
        private string cookieKey = "LoginUserName";
        private string DirectoryCookieKey = "DirectoryPathCookie";

        public HomeController(DBManager manager)
        {        
            _manager = manager;
        }

        public IActionResult Index()
        {
            var CookieValue = Request.Cookies[cookieKey];
            ViewData["username"] = CookieValue;
            return View();
        }
        public IActionResult SearchElastic(SearchModel Sm)
        {
            List<ElasticManager.WebsiteDetails> elastiSearchKeywordsList = new List<ElasticManager.WebsiteDetails>();
            string username;
            var CookieValue = Request.Cookies[cookieKey];
            if (CookieValue == null)
            {
                username = "";  
            }
            else
            {
                username = CookieValue.ToLower();
            }    
            elastiSearchKeywordsList = _ManagerElastic.search(username, Sm.Keywords);
            ViewData["username"] = CookieValue;
            ViewData["elastiSearchKeywordsList"] = elastiSearchKeywordsList;
            return View("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ConfigPage(string sPathMessage, bool bIncorrectPath, bool bDeletedCookie)
        {
            if (TempData["message"] != null)
            {
                ViewBag.Message = TempData["message"].ToString();
            }
            List<string> userWebsitesList = new List<string>();
            var CookieValue = Request.Cookies[cookieKey];

            userWebsitesList = _manager.RetrieveUserWebsites(CookieValue);
            ViewData["userWebsitesList"] = userWebsitesList;
            // display logout button on config page
            ViewData["username"] = CookieValue;


           // get cookie value for directory path cookie
           CookieValue = Request.Cookies[DirectoryCookieKey];

            // user submitted a directory
            if (sPathMessage != null)
            {
                // delete cookie and remove path
                if (bDeletedCookie == true)
                {
                    ViewData["DirectoryPathExist"] = "false";
                    ViewBag.message = sPathMessage;
                    return View();
                }
                // Path does not exist
                else if(bIncorrectPath == true)
                {
                    ViewData["DirectoryPathExist"] = "false";
                    ViewBag.message = sPathMessage;
                    return View();
                }              
                // path does exist
                else
                {
                    ViewData["DirectoryPathExist"] = "true";
                    ViewData["DirectoryPath"] = CookieValue;
                    ViewBag.message = sPathMessage;
                    return View();
                }
            }
            else
            {
                ViewData["DirectoryPathExist"] = "false";
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult UploadWebsites(ConfigPageModel cm)
        {
            var CookieValue = Request.Cookies[cookieKey];
            string textFromFile, websiteTitle, tenWebsiteWords;
            if (_manager.URLExist(cm.inputWebsite))
            {
                if (_manager.SaveWebsite(cm.inputWebsite, CookieValue))
                {
                    textFromFile = pythonScraper.Scrape(cm.inputWebsite);
                    if (textFromFile != "")
                    {
                        websiteTitle = _manager.getWebsiteTitle(cm.inputWebsite);
                        if (websiteTitle == "")
                        {
                            websiteTitle = "No title";
                        }

                        tenWebsiteWords = _manager.getTenWebsiteWords(textFromFile);

                        _ManagerElastic.addData(CookieValue.ToLower(), textFromFile, cm.inputWebsite, websiteTitle, tenWebsiteWords);
                        ViewBag.message = "Website was added";
                    }
                    else
                    {
                        _manager.RemoveWebsite(cm.inputWebsite, CookieValue);
                    }
                }
            }
            else
            {
                TempData["message"] = "Not a valid website, try again";
            }

            return RedirectToAction("ConfigPage");
        }
        public ActionResult RemoveWebsites(string website)
        {
            var CookieValue = Request.Cookies[cookieKey];
            if (_manager.RemoveWebsite(website, CookieValue))
            {
               _ManagerElastic.removeData(CookieValue.ToLower(), website);
            }
            return RedirectToAction("ConfigPage");
        }

        public ActionResult ProfilePage(ProfileModel profileMod)
        {
            var CookieValue = Request.Cookies[cookieKey];
            string username;
            ViewData["username"] = CookieValue;
            List<string> userProfileData = new List<string>();

            if (CookieValue == null)
            {
                username = "";
            }
            else
            {
                username = CookieValue;
                userProfileData = _manager.RetrieveUserInfoFromDB(profileMod, username);
                ViewData["userProfileData"] = userProfileData;
            }

            return View();
        }

        [HttpPost]
        public IActionResult UploadDirectory(string path)
        {
            string PathMessage = "";
            bool DeletedCookie = false, incorrectPath = false;

            // delete path
            if (path == null)
            {
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append(DirectoryCookieKey, "ExpireCookie", options);
                PathMessage = "Erased Saved Path";
                DeletedCookie = true;
            }
            // if directory does exist
            else if (Directory.Exists(path))
            {
                string key = DirectoryCookieKey;
                string value = path;

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Append(key, value, options);
                PathMessage = "Path was entered, scanning files";
            }
            // path does not exist
            else
            {
                PathMessage = "Try a different path, path does not exist";
                incorrectPath = true;
            }
            return RedirectToAction("ConfigPage", new { sPathMessage = PathMessage, bIncorrectPath = incorrectPath, bDeletedCookie = DeletedCookie });
        }
    }
}