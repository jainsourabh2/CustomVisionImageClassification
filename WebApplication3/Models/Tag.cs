using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace WebApplication3.Models
{
    public class Tag
    {
        public int ID
        {
            get; set;
        }

        [Required(ErrorMessage = "Please enter category")]
        public string Category
        {
            get; set;
        }

        [Required(ErrorMessage = "Please enter name")]
        public string Name
        {
            get; set;
        }

        [Required(ErrorMessage = "Please enter mapping string")]
        public string MapString
        {
            get; set;
        }

        public static IList<Tag> GetTagsFromDB()
        {
            string jsonString = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~\App_Data\Tag.json"));

            return (List<Tag>)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(List<Tag>));
        }
    }

    public static class  TagListExtension
    {
        public static string GetStrings(this IList<Tag> tags)
        {
            if (tags != null)
            {
                return string.Join(", ", tags.Select<Tag, string>(t => t.Name));
            }
            else
            {
                return "";
            }
        }
    }
}