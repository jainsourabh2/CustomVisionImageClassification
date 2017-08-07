using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Web;
using System.Web.Hosting;

namespace WebApplication3.Models
{
    public class Settings
    {
        private static Settings settings;

        private static string _rasaNLPUrl;
        private static bool _applyDummyVoice;
        private static string _dummyVoice;
        private static bool _applyDummyRASANLP;
        private static string _dummyRASANLPIntent;
        private static string _dummyRASANLPEntities;
        private static int? _maxHeightOrWidth;
        private static int? _minimumPredictionPercentage;
        private static bool _applyDummyPredictions;
        private static IList<DummyPrediction> _dummyPredictions;

        [JsonProperty]
        public static string RASANLPUrl
        {
            get
            {
                return _rasaNLPUrl;
            }
            set
            {
                _rasaNLPUrl = value;
            }
        }

        [JsonProperty]
        public static bool ApplyDummyVoice
        {
            get
            {
                return _applyDummyVoice;
            }
            set
            {
                _applyDummyVoice = value;
            }
        }

        [JsonProperty]
        public static string DummyVoice
        {
            get
            {
                return _dummyVoice;
            }
            set
            {
                _dummyVoice = value;
            }
        }

        [JsonProperty]
        public static bool ApplyDummyRASANLP
        {
            get
            {
                return _applyDummyRASANLP;
            }
            set
            {
                _applyDummyRASANLP = value;
            }
        }

        [JsonProperty]
        public static string DummyRASANLPIntent
        {
            get
            {
                return _dummyRASANLPIntent;
            }
            set
            {
                _dummyRASANLPIntent = value;
            }
        }

        [JsonProperty]
        public static string DummyRASANLPEntities
        {
            get
            {
                return _dummyRASANLPEntities;
            }
            set
            {
                _dummyRASANLPEntities = value;
            }
        }

        [JsonProperty]
        public static int MaxHeightOrWidth
        {
            get
            {
                return _maxHeightOrWidth ?? 500;
            }
            set
            {
                _maxHeightOrWidth = value;
            }
        }

        [JsonProperty]
        public static int MinimumPredictionPercentage
        {
            get
            {
                return _minimumPredictionPercentage ?? 60;
            }
            set
            {
                _minimumPredictionPercentage = value;
            }
        }

        [JsonProperty]
        public static bool ApplyDummyPredictions
        {
            get
            {
                return _applyDummyPredictions;
            }
            set
            {
                _applyDummyPredictions = value;
            }
        }

        [JsonProperty]
        public static IList<DummyPrediction> DummyPredictions
        {
            get
            {
                return _dummyPredictions;
            }
            set
            {
                _dummyPredictions = value;
            }
        }

        static Settings()
        {
            string jsonString = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~\App_Data\Settings.json"));

            settings = (Settings)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, typeof(Settings));
        }

        public static void Save()
        {
            System.IO.File.WriteAllText(HostingEnvironment.MapPath(@"~\App_Data\Settings.json"), JsonConvert.SerializeObject(settings));
        }
    }
}