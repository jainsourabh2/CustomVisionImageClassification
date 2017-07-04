using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using WebApplication3.Models;
using System.IO;
using WebApplication3.Enums;
using System.Configuration;

namespace WebApplication3.Controllers
{
    public class PredictionController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase ImageFile)
        {

            PredictionViewModel modelData = new PredictionViewModel();

            var predictionKey = "61923266b04e4173b6ff56c7bb2804bf";
            var projectID = "141bef78-2d42-45dc-9687-0d3b12ed702a";

            PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
            PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

            modelData.PredictionImageContentType = ImageFile.ContentType;


            byte[] thePictureAsBytes = new byte[ImageFile.ContentLength];

            using (BinaryReader theReader = new BinaryReader(ImageFile.InputStream))
            {
                thePictureAsBytes = theReader.ReadBytes(ImageFile.ContentLength);
            }

            modelData.PredictionImageBase64String = Convert.ToBase64String(thePictureAsBytes);


            using (MemoryStream stream = new MemoryStream(thePictureAsBytes))
            {
                modelData.PredictionResult = endpoint.PredictImage(Guid.Parse(projectID), stream);
            }

            modelData.Images = GetImagesByPrediction(modelData.PredictionResult);

            return View(modelData);
        }

        private IList<Image> GetImagesByPrediction(ImagePredictionResultModel results)
        {
            IList<Image> images = new List<Image>();

            string jsonString = System.IO.File.ReadAllText(Server.MapPath(@"~\App_Data\ImageDB.json"));

            IList<Image> dbImages = (List<Image>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(List<Image>));

            IList<int> strongTags = results.Predictions.Where(prediction => (prediction.Probability * 100) > int.Parse(ConfigurationManager.AppSettings["MinimumPredictionPercentage"])).Select<ImageTagPrediction, int>(strongPrediction => int.Parse(((Tag)Enum.Parse(typeof(Tag), strongPrediction.Tag)).ToString("D"))).ToList();

            foreach (Image dbImage in dbImages)
            {
                int tags = dbImage.Tags;
                bool display = false;
                foreach (int strongTag in strongTags)
                {
                    if ((strongTag & tags) == strongTag)
                    {
                        display = true;
                    }
                }

                if (display)
                {
                    images.Add(dbImage);
                }
            }

            return images;
        }
    }
}