using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Models;
using System.IO;
using Newtonsoft.Json;

namespace WebApplication3.Controllers
{
    public class TagImagesController : Controller
    {
        public ActionResult Index()
        {
            IList<Image> images = new List<Image>();

            string jsonString = System.IO.File.ReadAllText(Server.MapPath(@"~\App_Data\ImageDB.json"));

            List<Image> dbImages = (List<Image>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(List<Image>));

            string[] filenames = Directory.GetFiles(Server.MapPath(@"~\ImageFiles"));

            filenames.ToList().ForEach(fn =>
            {
                Image image = dbImages.FirstOrDefault(i => i.Filename == Path.GetFileName(fn));
                images.Add(new Image() { Filename = Path.GetFileName(fn), Tags = (image == null ? 0 : image.Tags) });
            });

            return View(images);
        }

        [HttpPost]
        public ActionResult Index(List<Image> images)
        {
            System.IO.File.WriteAllText(Server.MapPath(@"~\App_Data\ImageDB.json"), JsonConvert.SerializeObject(images));

            TempData["Message"] = "Updated successfully!";
            return RedirectToAction("Index");
        }
    }
}