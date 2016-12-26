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
                var device = ads.First();
                Controllers.HomeController.ad = device;
                device.MiniLibPath = Path.Combine(Directory.GetCurrentDirectory(), "MiniLib");

                device.InitMinicap();

                //device.StartMinicapServer();

                device.InitMiniTouch();
                
                //device.StartMiniTouchServer();
            } else {

            }

            var host = new WebHostBuilder()
                .UseKestrel()
                //.UseUrls("http://*:5000") //手动配置
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }



        

    }
}
