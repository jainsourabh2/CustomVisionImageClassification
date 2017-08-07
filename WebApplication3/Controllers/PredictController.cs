using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
//using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Helper;
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

            ViewData["ImageBase64String"] = Convert.ToBase64String(ResizePicture(ImageFile));

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

        [HttpPost]
        public ActionResult MinimumPredictionPercentage(int minimumPredictionPercentage, IList<Prediction> predictions)
        {
            IList<Tag> strongTags;
            IList<Image> images = GetImagesByPrediction(minimumPredictionPercentage, predictions, out strongTags);

            return Json(new { Images = images, StrongTagStrings = strongTags.GetStrings() });
        }

        [HttpPost]
        public ActionResult HandleVoice(string strongTagStrings, string voice)
        {
            if (voice != null && voice.Trim().Length > 0)
            {
                IList<Tag> voiceTags;
                string intent;
                string entities;
                IList<Image> images = GetImagesByVoice(voice, strongTagStrings, out voiceTags, out intent, out entities);

                if (images != null)
                {
                    return Json(new { ChangeInventory = true, Voice = voice, Images = images, StrongTagStrings = voiceTags.GetStrings(), Intent = intent, Entities = entities });
                }
            }

            return Json(new { ChangeInventory = false });
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

                    if (Settings.ApplyDummyPredictions)
                    {
                        modelData.Predictions = GetDummyPredictions();
                    }
                    else
                    {
                        modelData.Predictions = GetPredictions(endpoint.PredictImage(Guid.Parse(projectID), fileStream));
                    }


                    IList<Tag> strongTags;

                    modelData.Images = GetImagesByPrediction(Settings.MinimumPredictionPercentage, modelData.Predictions, out strongTags);

                    modelData.StrongTags = strongTags;
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

        public IList<Prediction> GetDummyPredictions()
        {
            IList<Prediction> predictions = new List<Prediction>();

            IList<Tag> tags = Tag.GetTagsFromDB();


            foreach (Tag tag in tags)
            {
                double dummyPrediction;

                dummyPrediction = Settings.DummyPredictions.Where(dp => dp.Key.ToLower().Trim() == tag.Name.ToLower().Trim()).FirstOrDefault()?.Value ?? GetDummyDouble(tag.ID);

                predictions.Add(new Prediction()
                {
                    MappingTagID = tag.ID,
                    MappingTagString = tag.Name,
                    Probability = dummyPrediction,
                    Tag = tag.MapString
                });
            }


            return predictions;
        }

        public double GetDummyDouble(int id)
        {
            switch (id)
            {
                case 1:
                    return .75;
                case 2:
                    return .70;
                case 4:
                    return .56;
                case 8:
                    return .30;
                case 16:
                    return .55;
                case 32:
                    return .75;
                default:
                    return .90;
            }
        }

        public IList<Prediction> GetPredictions(ImagePredictionResultModel results)
        {
            IList<Prediction> predictions = new List<Prediction>();

            IList<Tag> tags = Tag.GetTagsFromDB();


            foreach (ImageTagPrediction prediction in results.Predictions)
            {
                predictions.Add(new Prediction() {
                                                    Tag = prediction.Tag,
                                                    Probability = prediction.Probability,
                                                    MappingTagID = tags.First(tm => tm.MapString.ToString() == prediction.Tag).ID,
                                                    MappingTagString = tags.First(tm => tm.MapString.ToString() == prediction.Tag).Name
                });
            }

            return predictions;
        }

        private IList<Image> GetImagesByPrediction(int minimumPredictionPercentage, IList<Prediction> predictions, out IList<Tag> strongTags)
        {
            IList<Image> dbImages = Image.GetImagesFromDB();

            IList<Tag> tags = Tag.GetTagsFromDB();

            IList<int> strongTagIDs = new List<int>();


            var groupByCategory = predictions.Where(
                                            prediction => (prediction.Probability * 100) >= minimumPredictionPercentage
                                        ).Join(
                                                tags, 
                                                prediction => prediction.MappingTagID, 
                                                tag => tag.ID, 
                                                (prediction, tag) => new { Tag = tag, Prediction = prediction }
                                            ).GroupBy(joinResult => joinResult.Tag.Category);

            foreach (var group in groupByCategory)
            {
                strongTagIDs.Add(group.OrderByDescending(line => line.Prediction.Probability).First().Tag.ID);
            }


            strongTags = tags.Where(tag => strongTagIDs.Count > 0 && strongTagIDs.Contains(tag.ID)).ToList();

            return dbImages.Where(image => strongTagIDs.All(strongTag => (strongTag & image.Tags) == strongTag)).ToList();
        }

        private IList<Image> GetImagesByVoice(string voice, string strongTagStrings, out IList<Tag> voiceTags, out string intent, out string entities)
        {
            intent = "None";
            entities = "";

            voiceTags = Tag.GetTagsFromDB().Where(t => strongTagStrings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(val => val.Trim().ToLower()).Contains(t.Name.Trim().ToLower())).ToList();

            VoiceResponse voiceResponse = null;
            if (Settings.ApplyDummyRASANLP)
            {
                voiceResponse = new VoiceResponse() {
                    intent = new VoiceResponseIntent() { name = Settings.DummyRASANLPIntent },
                    entities = Settings.DummyRASANLPEntities.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select<string, VoiceResponseEntity>(entity => new VoiceResponseEntity { value = entity.Trim() }).ToList()
                };
            }
            else
            {
                voiceResponse = SendVoiceRequest(voice);
            }

            if (voiceResponse != null && voiceResponse.intent != null && voiceResponse.entities != null && voiceResponse.entities.Count > 0)
            {
                intent = voiceResponse.intent.name;

                foreach (VoiceResponseEntity entity in voiceResponse.entities)
                {
                    entities += (entities.Length > 0 ? ", " : "") + entity.value;

                    Tag tag = Tag.GetTagsFromDB().FirstOrDefault(t => t.Name.Trim().ToLower() == entity.value.Trim().ToLower());


                    if (tag != null)
                    {
                        bool contains = voiceTags.FirstOrDefault(t => t.Name == tag.Name) != null;

                        if (voiceResponse.intent.name.Trim().ToLower() == "show" && !contains)
                        {
                            voiceTags.Add(tag);
                        }
                        else if (voiceResponse.intent.name.Trim().ToLower() == "remove" && contains)
                        {
                            voiceTags.Remove(voiceTags.First(t => t.Name == tag.Name));
                        }
                    }
                }

                Dictionary<string, int> db = Tag.GetTagsFromDB().GroupBy(tag => tag.Category).ToDictionary(g => g.Key, g => g.Count());

                var categories = voiceTags.GroupBy(tag => tag.Category);

                foreach (var category in categories)
                {
                    if (category.Count() == db[category.Key])
                    {
                        foreach (Tag tag in category)
                        {
                            voiceTags.Remove(voiceTags.First(t => t.Name == tag.Name));
                        }
                    }
                }

                IList<int> strongTagIDs = voiceTags.Select<Tag, int>(t => t.ID).ToList();

                return Image.GetImagesFromDB().Where(image => strongTagIDs.All(strongTag => (strongTag & image.Tags) == strongTag)).ToList();
            }

            return null;
        }

        private VoiceResponse SendVoiceRequest(string voice)
        {
            var client = new RestClient();
            client.EndPoint = Settings.RASANLPUrl;
            client.Method = HttpVerb.POST;
            client.PostData = "{\"q\":\""+ voice + "\"}";

            var json = client.MakeRequest();

            return (VoiceResponse)Newtonsoft.Json.JsonConvert.DeserializeObject(json, typeof(VoiceResponse));
        }

        private byte[] ResizePicture(HttpPostedFileBase ImageFile)
        {
            string originalFilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(ImageFile.FileName));
            string resizedFilename = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(ImageFile.FileName));

            try
            {
                int currentWidth = 0;
                int currentHeight = 0;
                int newWidth = 0;
                int newHeight = 0;
                int maxHeightOrWidth = Settings.MaxHeightOrWidth;

                ImageFile.SaveAs(originalFilename);

                using (System.Drawing.Image image = System.Drawing.Image.FromFile(originalFilename))
                {
                    currentWidth = image.Size.Width;
                    currentHeight = image.Size.Height;

                    if (currentWidth > maxHeightOrWidth || currentHeight > maxHeightOrWidth)
                    {
                        if (currentWidth > currentHeight)
                        {
                            newWidth = maxHeightOrWidth;
                            newHeight = 0;
                        }
                        else
                        {
                            newWidth = 0;
                            newHeight = maxHeightOrWidth;
                        }
                    }
                    else
                    {
                        newWidth = currentWidth;
                        newHeight = currentHeight;
                    }

                    ResizePicture(originalFilename, resizedFilename, newWidth, newHeight, 100);

                    return System.IO.File.ReadAllBytes(resizedFilename);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (System.IO.File.Exists(originalFilename))
                {
                    System.IO.File.Delete(originalFilename);
                }

                if (System.IO.File.Exists(resizedFilename))
                {
                    System.IO.File.Delete(resizedFilename);
                }
            }
        }

        private void ResizePicture(string Org, string Des, int FinalWidth, int FinalHeight, int ImageQuality)
        {
            System.Drawing.Bitmap NewBMP;
            System.Drawing.Graphics graphicTemp;
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(Org);

            int iWidth;
            int iHeight;
            if ((FinalHeight == 0) && (FinalWidth != 0))
            {
                iWidth = FinalWidth;
                iHeight = (bmp.Size.Height * iWidth / bmp.Size.Width);
            }
            else if ((FinalHeight != 0) && (FinalWidth == 0))
            {
                iHeight = FinalHeight;
                iWidth = (bmp.Size.Width * iHeight / bmp.Size.Height);
            }
            else
            {
                iWidth = FinalWidth;
                iHeight = FinalHeight;
            }

            NewBMP = new System.Drawing.Bitmap(iWidth, iHeight);
            graphicTemp = System.Drawing.Graphics.FromImage(NewBMP);
            graphicTemp.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            graphicTemp.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            graphicTemp.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphicTemp.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphicTemp.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphicTemp.DrawImage(bmp, 0, 0, iWidth, iHeight);
            graphicTemp.Dispose();
            System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
            System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ImageQuality);
            encoderParams.Param[0] = encoderParam;
            System.Drawing.Imaging.ImageCodecInfo[] arrayICI = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            for (int fwd = 0; fwd <= arrayICI.Length - 1; fwd++)
            {
                if (arrayICI[fwd].FormatDescription.Equals("JPEG"))
                {
                    NewBMP.Save(Des, arrayICI[fwd], encoderParams);
                }
            }

            NewBMP.Dispose();
            bmp.Dispose();
        }
    }
}