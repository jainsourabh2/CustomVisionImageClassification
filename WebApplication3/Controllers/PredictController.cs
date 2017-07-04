using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
//using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Enums;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class PredictController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase ImageFile)
        {
            ViewData["ImageContentType"] = ImageFile.ContentType;
            ViewData["ImageContentLength"] = ImageFile.ContentLength;
            ViewData["ImageExtension"] = Path.GetExtension(ImageFile.FileName);


            byte[] thePictureAsBytes = new byte[ImageFile.ContentLength];

            using (BinaryReader theReader = new BinaryReader(ImageFile.InputStream))
            {
                thePictureAsBytes = theReader.ReadBytes(ImageFile.ContentLength);
            }

            ViewData["ImageBase64String"] = Convert.ToBase64String(thePictureAsBytes);

            return View();
        }

        [HttpPost]
        public ActionResult Predict(PredictRequest request)
        {
            PredictionViewModel modelData = new PredictionViewModel();

            byte[] thePictureAsBytes = new byte[request.ImageContentLength];

            thePictureAsBytes = Convert.FromBase64String(request.ImageBase64String);

            using (MemoryStream stream = new MemoryStream(thePictureAsBytes))
            {
                using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(stream))
                {
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle(request.ImageCoordsX, request.ImageCoordsY, (request.ImageCoordsX2 - request.ImageCoordsX), (request.ImageCoordsY2 - request.ImageCoordsY));
                    System.Drawing.Bitmap cropped = bmp.Clone(rect, bmp.PixelFormat);

                    using (MemoryStream croppedStream = new MemoryStream())
                    {
                        string filename = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + request.ImageExtension);

                        cropped.Save(filename);

                        Predict(filename, request.ImageContentType, modelData);
                    }
                }
            }

            return Json(modelData);
        }

        private void Predict(string filename, string contentType, PredictionViewModel modelData)
        {
            try
            {
                modelData.PredictionImageBase64String = Convert.ToBase64String(System.IO.File.ReadAllBytes(filename));
                modelData.PredictionImageContentType = contentType;

                using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var predictionKey = "61923266b04e4173b6ff56c7bb2804bf";
                    var projectID = "141bef78-2d42-45dc-9687-0d3b12ed702a";

                    PredictionEndpointCredentials predictionEndpointCredentials = new PredictionEndpointCredentials(predictionKey);
                    PredictionEndpoint endpoint = new PredictionEndpoint(predictionEndpointCredentials);

                    modelData.PredictionResult = endpoint.PredictImage(Guid.Parse(projectID), fileStream);

                    modelData.Images = GetImagesByPrediction(modelData.PredictionResult);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (System.IO.File.Exists(filename))
                {
                    System.IO.File.Delete(filename);
                }
            }
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