using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SNHU_Search.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SNHU_Search.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBManager _manager;
        private readonly ElasticManager _ManagerElastic = new ElasticManager();
        private string cookieKey = "LoginUserName";
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
            List<string> elastiSearchKeywordsList = new List<string>();
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

        public IActionResult ConfigPage()
        {
            List<string> userWebsitesList = new List<string>();
            var CookieValue = Request.Cookies[cookieKey];

            userWebsitesList = _manager.RetrieveUserWebsites(CookieValue);
            ViewData["userWebsitesList"] = userWebsitesList;
            // display logout button on config page
            ViewData["username"] = CookieValue;
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
            _manager.SaveWebsite(cm.inputWebsite, CookieValue);
            _ManagerElastic.addData(CookieValue.ToLower(), "test", cm.inputWebsite);
            return RedirectToAction("ConfigPage");
        }
    }
}
