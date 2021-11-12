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
        public HomeController(DBManager manager)
        {        
            _manager = manager;
        }

        public IActionResult Index()
        {
            string key = "LoginUserName";
            var CookieValue = Request.Cookies[key];
            ViewData["username"] = CookieValue;
            return View();
        }
        public IActionResult SearchElastic(SearchModel Sm)
        {
            List<string> elastiSearchKeywordsList = new List<string>();
            elastiSearchKeywordsList = _ManagerElastic.search(Sm.Keywords);
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
            string key = "LoginUserName";
            var CookieValue = Request.Cookies[key];

            userWebsitesList = _manager.RetrieveUserWebsites(CookieValue);
            ViewData["userWebsitesList"] = userWebsitesList;
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
            string key = "LoginUserName";
            var CookieValue = Request.Cookies[key];
            _manager.SaveWebsite(cm.inputWebsite, CookieValue);
            return RedirectToAction("ConfigPage");
        }
    }
}
