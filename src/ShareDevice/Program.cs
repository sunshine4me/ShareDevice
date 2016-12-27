using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Diagnostics;
using Devices;
using Microsoft.Extensions.Configuration;

namespace ShareDevice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            

            if(Environment.ExpandEnvironmentVariables("%ANDROID_HOME%")== "%ANDROID_HOME%"){
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("warring : 未检测 %ANDROID_HOME% 环境变量!");
            }

            var ads = AndroidDevice.getAllDevices();
            if (ads.Count > 0) {
                
                var device = ads.First();

                Console.WriteLine($"开始共享手机 {device.deviceName} ...");

                Controllers.HomeController.ad = device;
                device.MiniLibPath = Path.Combine(Directory.GetCurrentDirectory(), "MiniLib");

                device.InitMinicap();

                //device.StartMinicapServer();

                device.InitMiniTouch();
                
                //device.StartMiniTouchServer();
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("error:没有发现可用设备.按任意键结束...");
                Console.ReadKey();
                return;
            }


            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("hosting.json", optional: true)
               .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                //.UseUrls("http://*:5020") //手动配置
                .UseConfiguration(config) //配置文件
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }



        

    }
}
