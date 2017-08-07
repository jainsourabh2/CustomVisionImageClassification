using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Cognitive.CustomVision.Models;
namespace WebApplication3.Models
{
    public class PredictionViewModel
    {
        private IList<Prediction> _predictions;
        private IList<Tag> _strongTags;
        private IList<Image> _images;
        private string _predictionImageBase64String;
        private string _predictionImageContentType;
        
        public IList<Prediction> Predictions
        {
            get
            {
                return _predictions;
            }
            set
            {
                _predictions = value;
            }
        }

        public IList<Tag> StrongTags
        {
            get
            {
                return _strongTags;
            }
            set
            {
                _strongTags = value;
            }
        }

        public string StrongTagStrings
        {
            get
            {
                return _strongTags.GetStrings();
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
                return Settings.MinimumPredictionPercentage.ToString();
            }
        }
    }
}