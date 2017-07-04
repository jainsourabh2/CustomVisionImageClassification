using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Cognitive.CustomVision.Models;
namespace WebApplication3.Models
{
    public class PredictionViewModel
    {
        private ImagePredictionResultModel _predictionResult;
        private IList<Image> _images;
        private string _predictionImageBase64String;
        private string _predictionImageContentType;
        
        public ImagePredictionResultModel PredictionResult
        {
            get
            {
                return _predictionResult;
            }
            set
            {
                _predictionResult = value;
            }
        }

        public IList<Image> Images
        {
            get
            {
                return _images;
            }
            set
            {
                _images = value;
            }
        }

        public string PredictionImageBase64String
        {
            get
            {
                return _predictionImageBase64String;
            }
            set
            {
                _predictionImageBase64String = value;
            }
        }

        public string PredictionImageContentType
        {
            get
            {
                return _predictionImageContentType;
            }
            set
            {
                _predictionImageContentType = value;
            }
        }

        public string MinimumPredictionPercentage
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["MinimumPredictionPercentage"];
            }
        }
    }
}