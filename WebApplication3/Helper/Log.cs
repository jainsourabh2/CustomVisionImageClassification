using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace WebApplication3.Helper
{
    public class Log
    {
        public static void LogMessage(string s)
        {
            File.AppendAllText(HostingEnvironment.MapPath(@"~\App_Data\Log.txt"), DateTime.Now.ToString() + ": " + s + "<br /><br />");
        }

        public static string GetLogs()
        {
            return File.ReadAllText(HostingEnvironment.MapPath(@"~\App_Data\Log.txt"));
        }
    }
}