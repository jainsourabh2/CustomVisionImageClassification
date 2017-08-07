using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class VoiceResponse
    {
        public VoiceResponseIntent intent
        {
            get; set;
        }
        
        public IList<VoiceResponseEntity> entities
        {
            get; set;
        }
    }

    public class VoiceResponseEntity
    {
        public string value
        {
            get; set;
        }
    }

    public class VoiceResponseIntent
    {
        public string name
        {
            get; set;
        }
    }
}