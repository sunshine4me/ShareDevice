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
using System.Threading;
using System.Runtime.InteropServices;

namespace ShareDevice
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.White;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            //bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

            if (Environment.ExpandEnvironmentVariables("%ANDROID_HOME%")== "%ANDROID_HOME%"){
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("warring : 未检测到 %ANDROID_HOME% 环境变量,启用本地adb程序!");
                Console.ForegroundColor = ConsoleColor.White;
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    AndroidDevice.adbFile = Path.Combine(Directory.GetCurrentDirectory(), "MiniLib/adb.exe");
                } else {
                    AndroidDevice.adbFile = Path.Combine(Directory.GetCurrentDirectory(), "MiniLib/adb");
                }
            }

            var ads = AndroidDevice.getAllDevices();
            if (ads.Count > 0) {
                
                var device = ads.First();

                Console.WriteLine($"开始共享手机 {device.deviceName} ...");

                Controllers.WSController.ad = device;
                device.MiniLibPath = Path.Combine(Directory.GetCurrentDirectory(), "MiniLib");

                device.InitMinicap();

                device.StartMinicapServer();

                device.InitMiniTouch();
                
                device.StartMiniTouchServer();


                Thread.Sleep(3000);
                device.StartMinicap();
                device.StartMiniTouch();

            } else {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("warring:未发现可用设备.输入\"Y\"可进入查看录像模式,输入其他将结束本程序...");
                Console.ForegroundColor = ConsoleColor.White;
                
                var rs = Console.ReadLine().ToString();
                var  yn = rs.FirstOrDefault();
                if(yn !='Y' && yn != 'y') {
                    return;
                }
               
               
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


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(">>>>>>>>> 服务器开启成功,请访问 http://电脑IP:5020 进行访问!");
            Console.ForegroundColor = ConsoleColor.White;

            host.Run();
        }



        

    }
}
