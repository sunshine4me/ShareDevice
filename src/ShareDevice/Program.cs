using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Diagnostics;
using Devices;

namespace ShareDevice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);




            var ads = AndroidDevice.getAllDevices();
            if (ads.Count > 0) {
                Controllers.HomeController.ad = ads.First();
                Controllers.HomeController.ad.MiniLibPath = Path.Combine(Directory.GetCurrentDirectory(), "MiniLib");

                Controllers.HomeController.ad.InitMinicap();
            }

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }



        

    }
}
