﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SNHU_Search.Models;
using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

namespace SNHU_Search.Controllers
{
    public class HomeController : Controller
    {

        private string cookieKey = "LoginUserName";
        private string DirectoryCookieKey = "DirectoryPathCookie";              
        private string CookieSkippedDirFilesKey = "DirectorySkippedFilesCookie";                                  
        private readonly DBManager _manager;
        private readonly ElasticManager _ManagerElastic = new ElasticManager();
        private readonly DirectoryManager _ManagerDirectory = new DirectoryManager();
        private readonly PythonModel pythonScraper = new PythonModel();
        private readonly IWebHostEnvironment webHostEnvironment;
        public HomeController(DBManager manager, IWebHostEnvironment hostEnvironment)
        {
            _manager = manager;
            webHostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            var CookieValue = Request.Cookies[cookieKey];
            ViewData["username"] = CookieValue;
            return View();
        }
        public IActionResult SearchElastic(SearchModel Sm)
        {
            List<ElasticManager.WebsiteDetails> elasticSearchKeywordsList = new List<ElasticManager.WebsiteDetails>();
            List<ElasticManager.WebsiteDetails> elasticSearchListDirectory = new List<ElasticManager.WebsiteDetails>();
            List<ElasticManager.WebsiteDetails> elasticSearchListWebsites = new List<ElasticManager.WebsiteDetails>();
            List<ElasticManager.WebsiteDetails> elasticSearchListFileLocation = new List<ElasticManager.WebsiteDetails>();

            string username;
            var CookieValue = Request.Cookies[cookieKey];
            if (CookieValue == null)
            {
                username = "";
            }
            else
            {
                username = CookieValue.ToLower();
                _manager.UploadKeywordForAnalytics(Sm.Keywords, username); //to add keyword to global list
            }
            elasticSearchKeywordsList = _ManagerElastic.search(username, Sm.Keywords);
            // search for keywords in directory instance
            elasticSearchListDirectory = _ManagerElastic.search(_ManagerDirectory.getElasticSearchIndexName() + username, Sm.Keywords);
            List<ElasticManager.WebsiteDetails> search = elasticSearchKeywordsList.Concat(elasticSearchListDirectory).ToList();

            for (int i = 0; i < search.Count(); i++)
            {
                bool isUri = Uri.IsWellFormedUriString(search[i].URL, UriKind.RelativeOrAbsolute);
                if (isUri)
                {
                    elasticSearchListWebsites.Add(search[i]);
                }
                else
                {
                    elasticSearchListFileLocation.Add(search[i]);
                }
            }


            ViewData["username"] = CookieValue;
            ViewData["elasticSearchKeywordsWebsitesList"] = elasticSearchListWebsites;
            ViewData["elasticSearchKeywordsFileLocationList"] = elasticSearchListFileLocation;

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

            // get cookie value for skipped files
            var cookieSkippedFilesValue = Request.Cookies[CookieSkippedDirFilesKey];

            if (bDeletedCookie == true)
            {
                ViewData["DirectoryPathExist"] = "false";
                ViewBag.message = sPathMessage;

                return View();
            }
            // Path does not exist
            if (bIncorrectPath == true)
            {
                ViewData["DirectoryPathExist"] = "false";
                ViewBag.message = sPathMessage;
                return View();
            }
            // cookie was deleted
            else if (CookieValue == null)
            {
                ViewData["DirectoryPath"] = CookieValue;
                ViewData["DirectoryPathExist"] = "false";
            }
            // path does exist
            else
            {
                ViewData["DirectoryPathExist"] = "true";
                ViewData["DirectoryPath"] = CookieValue;
                ViewData["SkippedFiles"] = cookieSkippedFilesValue;
                ViewBag.message = sPathMessage;
                return View();
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
            int skippedFiles = 0;

            // clear directory path, cookie and ElasticSearch
            if (path == null)
            {
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append(DirectoryCookieKey, "ExpireCookie", options);
                Response.Cookies.Append(CookieSkippedDirFilesKey, "ExpireCookie", options);
                PathMessage = "Erased Saved Path";
                DeletedCookie = true;

                // delete index from elasticsearch
                string currentUsername = getCookieUsername();

                // combine username with specific name in Directory Manager
                string ElasticIndexName = _ManagerDirectory.getElasticSearchIndexName() + currentUsername;
                _ManagerElastic.removeIndexDirectory(ElasticIndexName);
            }
            // if directory does exist
            else if (System.IO.Directory.Exists(path))
            {
                Startup.Progress = 0;

                string key = DirectoryCookieKey;
                string value = path;

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Append(key, value, options);
                PathMessage = "Path was entered.";

                // begin scanning files on computer
                scanDirectory(path);

                // below saves number of skipped files in a cookie
                string keySkippedFiles = CookieSkippedDirFilesKey;
                skippedFiles = _ManagerDirectory.getFilesSkipped();
                string numSkippedFiles = skippedFiles.ToString();

                CookieOptions optionSkipped = new CookieOptions();
                options.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Append(keySkippedFiles, numSkippedFiles, optionSkipped);

            }
            // path does not exist
            else
            {
                PathMessage = "Try a different path, path does not exist";
                incorrectPath = true;
            }
            return RedirectToAction("ConfigPage", new { sPathMessage = PathMessage, bIncorrectPath = incorrectPath, bDeletedCookie = DeletedCookie});
        }

        string getCookieUsername()
        {
            string username = "username";
            var CookieValue = Request.Cookies[cookieKey];
            if (CookieValue != null)
            {
                username = CookieValue.ToLower();
            }
            return username;
        }
        string getCookieSkippedFiles()
        {
            string skippedFiles = "skipped";
            var CookieValue = Request.Cookies[CookieSkippedDirFilesKey];
            if (CookieValue != null)
            {
                skippedFiles = CookieValue.ToLower();
            }
            return skippedFiles;
        }
        void scanDirectory(string directoryPath)
        {
            string username = getCookieUsername();

            _ManagerDirectory.setPath(directoryPath);
            _ManagerDirectory.setUsername(username);
            _ManagerDirectory.scan();
        }

        public IActionResult ChangePassword(ChangePasswordModel changePasswordMod)
        {
            var CookieValue = Request.Cookies[cookieKey];
            string username;
            ViewData["username"] = CookieValue;

            // Checks if all required fields are met
            if (ModelState.IsValid)
            {
                if (changePasswordMod.userNewPassword.Length < 4 && changePasswordMod.userConfirmNewPassword.Length < 4)
                    ViewBag.message = "Password must be greater than 4 characters!";
                else
                {
                    username = CookieValue;
                    _manager.UserChangesPassword(changePasswordMod, username);
                    ViewBag.message = "Password updated successfully!";
                }
            }
            return View("~/Views/Login/ChangePassword.cshtml");
        }
        [HttpPost]
        public ActionResult Progress()
        {
            return this.Content(Startup.Progress.ToString());
        }

        public IActionResult AnalyticsPage()
        {
            // Pulls the list of strings from database, formatted ["term1", "1", "term2", "2"], where any int = #times of searched term
            List<string> data = new List<string>();
            data = _manager.AnalyticKeywordsForUser(getCookieUsername());

            List<string> terms = new List<string>();
            // Let's split that info up now
            for (int i = 0; i < data.Count - 1; i+= 2) {
                // Words
                terms.Add(data[i]);
            }

            List<int> counts = new List<int>();
            for (int i = 1; i < data.Count; i+= 2) {
                //counts.Add(ToInt32(data[i]));
                int x = 0;
                Int32.TryParse(data[i], out x);
                counts.Add(x);
            }

            // temporary data just to make sure we have 6 points of data to work with
            while (true) {
                if (counts.Count == 6) break;

                counts.Add(1);
            }

            ViewBag.Counts = counts;
            ViewData["terms"] = terms;




            // Temp data just to fill chart, replace with actual
            //int[] counts = { 1, 1, 1, 1, 1, 1}; // Default initialization for effect, 0's for words, 1's for repeats
            //ViewBag.Counts = counts;

            //string[] terms = { "0", "0", "0", "0", "0", "0" };
            //ViewBag.Words = terms;

            //ViewData["terms"] = "string";

            var CookieValue = Request.Cookies[cookieKey];
            ViewData["username"] = CookieValue;
            return View("~/Views/Home/AnalyticsPage.cshtml");
        }
    }
}