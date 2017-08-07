using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class SettingsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string RASANLPUrl, bool ApplyDummyVoice, string DummyVoice, bool ApplyDummyRASANLP, string DummyRASANLPIntent, string DummyRASANLPEntities, int MaxHeightOrWidth, int MinimumPredictionPercentage, bool ApplyDummyPredictions)
        {
            Settings.ApplyDummyVoice = ApplyDummyVoice;
            Settings.DummyVoice = DummyVoice;
            Settings.RASANLPUrl = RASANLPUrl;
            Settings.ApplyDummyRASANLP = ApplyDummyRASANLP;
            Settings.DummyRASANLPIntent = DummyRASANLPIntent;
            Settings.DummyRASANLPEntities = DummyRASANLPEntities;
            Settings.MaxHeightOrWidth = MaxHeightOrWidth;
            Settings.MinimumPredictionPercentage = MinimumPredictionPercentage;
            Settings.ApplyDummyPredictions = ApplyDummyPredictions;

            Settings.Save();

            TempData["Message"] = "Updated successfully!";
            return RedirectToAction("Index");
        }
    }
}