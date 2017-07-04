using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class PredictRequest
    {
        public string ImageBase64String
        {
            get; set;
        }

        public string ImageContentType
        {
            get; set;
        }

        public int ImageContentLength
        {
            get; set;
        }

        public string ImageExtension
        {
            get; set;
        }

        public int ImageCoordsX
        {
            get; set;
        }

        public int ImageCoordsY
        {
            get; set;
        }

        public int ImageCoordsX2
        {
            get; set;
        }

        public int ImageCoordsY2
        {
            get; set;
        }
    }
}