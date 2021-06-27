using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nice.Network.Web.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer httpServer = new HttpServer(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Web"), "http://127.0.0.1:8091/","http://www.nicecode.club");
            httpServer.Start();

            ControllerFactory.RegisterRoutes();
            Process.Start("explorer.exe", "http://127.0.0.1:8091/");
        }
    }
}
