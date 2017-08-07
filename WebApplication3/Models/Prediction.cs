using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Prediction
    {
        public string Tag
        {
            get; set;
        }

        public double Probability
        {
            get; set;
        }

        public int MappingTagID
        {
            get; set;
        }

        public string MappingTagString
        {
            get; set;
        }
    }
}