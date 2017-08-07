using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace WebApplication3.Models
{
    public class Image
    {
        public string Filename
        {
            get; set;
        }

        public int Tags
        {
            get; set;
        }

        public static IList<Image> GetImagesFromDB()
        {
            string jsonString = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~\App_Data\ImageDB.json"));

            return (List<Image>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(List<Image>));
        }

        public static void SaveImagestoDB(IList<Image> images)
        {
            System.IO.File.WriteAllText(HostingEnvironment.MapPath(@"~\App_Data\ImageDB.json"), JsonConvert.SerializeObject(images));
        }
    }
}