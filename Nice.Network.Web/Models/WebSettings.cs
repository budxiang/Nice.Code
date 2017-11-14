
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.Network.Web.Models
{
    internal class WebSettings
    {
        public static string DefaultPage = "index.html";

        public static string ViewsPath = "Views";

        public static string WebControllerAssembly = null;

        public static string WebControllerNamespace = null;

        public static int SessionTimeout = 1200;
    
    }
}
