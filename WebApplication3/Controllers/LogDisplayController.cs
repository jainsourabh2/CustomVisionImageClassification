using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Helper;

namespace WebApplication3.Controllers
{
    public class LogDisplayController : Controller
    {
        // GET: LogDisplay
        public ActionResult Index()
        {
            ViewData["Logs"] = Log.GetLogs() ?? "No logs.";
            return View();
        }
    }
}