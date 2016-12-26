using Minicap;
using MiniTouch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Devices
{
    public class AndroidDevice {

        /// <summary>
        /// CPU abi版本
        /// </summary>
        private string abi;
        /// <summary>
        /// 系统SDK版本
        /// </summary>
        private string sdk;

        private MinicapStream minicap;

        private MiniTouchStream minitouch;

        private Process minicapServerProcess;

        private Process minitouhServerProcess;

        readonly string jarpath = "/data/local/tmp";

        public string deviceName { get; set; }

        /// <summary>
        /// 相关库文件的地址
        /// </summary>
        public string MiniLibPath { get; set; }


        /// <summary>
        /// 手机实际像素 宽
        /// </summary>
        private int width;
        /// <summary>
        /// 手机实际像素 高
        /// </summary>
        private int height;
        /// <summary>
        /// 图像输出宽度
        /// </summary>
        public int virtualwidth {  get; private set; }
        /// <summary>
        /// 图像输出高度
        /// </summary>
        public int virtualheight { get; private set; }

        /// <summary>
        /// 图像输出比
        /// </summary>
        public int scale { get; set; }


        readonly private string GET_SIZE_COMMAND = "shell dumpsys window windows | grep mScreenRect";
        readonly private string GET_DEVICE_ABI_COMMAND = "shell getprop ro.product.cpu.abi";
        readonly private string GET_DEVICE_SDK_COMMAND = "shell getprop ro.build.version.sdk";


        readonly private int orientation = 0;//旋转角度?


        public AndroidDevice(string _deviceName) {

            scale = 3;//默认比例1:3

            deviceName = _deviceName;

            abi = adbByDevice(GET_DEVICE_ABI_COMMAND).Result.Trim();
            sdk = adbByDevice(GET_DEVICE_SDK_COMMAND).Result.Trim();


            var result = adbByDevice(GET_SIZE_COMMAND).Result;
            Match match = Regex.Match(result, @"\d{3,4}\,\d{3,4}");
            string size = match.Groups[0].Value;
            width = Convert.ToInt32(size.Split(',').ToArray()[0]);
            height = Convert.ToInt32(size.Split(',').ToArray()[1]);
            virtualwidth = width / scale;
            virtualheight = height / scale;


            

        }


        

        public MinicapEventHandler SetMinicapEvent {
            set {
                minicap.clearPushEvent();
                minicap.push += value;
            }
        }

        

        public void InitMinicap() {

            adbByDevice("forward --remove tcp:1313").Wait();

            minicap = new MinicapStream();
            

            var MINICAP_FILE_PATH = Path.Combine(MiniLibPath, $"minicap/bin/{abi}/minicap");
            var MINICAPSO_FILE_PATH = Path.Combine(MiniLibPath, $"minicap/shared/android-{sdk}/{abi}/minicap.so");


            pushFile(MINICAP_FILE_PATH, jarpath);
            pushFile(MINICAPSO_FILE_PATH, jarpath);

            adbByDevice($"shell chmod 777 {jarpath}/minicap").Wait();


            //Shell("adb", "forward --remove-all").Wait();

            string command = $"forward tcp:{minicap.PORT} localabstract:minicap";
           
            adbByDevice(command).Wait();


            



        }


        public void StartMinicapServer() {

            string tmp = $"-s {deviceName} shell LD_LIBRARY_PATH={jarpath} /data/local/tmp/minicap -P {width}x{height}@{virtualwidth}x{virtualheight}/{orientation}";
            //string tmp = string.Format("shell LD_LIBRARY_PATH=/data/local/tmp /data/local/tmp/minicap -P 1080x1920@360x640/0");

            //启动server
            minicapServerProcess = StartProcess("adb", tmp);

        }

        /// <summary>
        /// 启动截图相关服务
        /// </summary>
        public void StartMinicap() {
            minicap.Run();
        }

        public void StopMinicap() {
            minicap.Stop();
            try {
                minicapServerProcess.Kill();
            } catch (Exception) {

            }
        }




        public void InitMiniTouch() {

            adbByDevice("forward --remove tcp:1111").Wait();

            minitouch = new MiniTouchStream();

            var MINITOUCH_FILE_PATH = Path.Combine(MiniLibPath, $"minitouch/{abi}/minitouch");

            pushFile(MINITOUCH_FILE_PATH, jarpath);

            adbByDevice($"shell chmod 777 {jarpath}/minitouch").Wait();


            string forward = string.Format("forward tcp:{0} localabstract:minitouch", minitouch.PORT);

            adbByDevice(forward).Wait();

            



        }

        public void StartMiniTouchServer() {
            string serverCommand = $"-s {deviceName} shell {jarpath}/minitouch";

            //启动server
            minitouhServerProcess = StartProcess("adb", serverCommand);
            
        }

        /// <summary>
        /// 启动点击相关的服务
        /// </summary>
        public void StartMiniTouch() {

            minitouch.Start();
        }

        public void StopMiniTouch() {
            minitouch.Stop();
            try {
                minitouhServerProcess.Kill();
            } catch (Exception) {

            }
        }


        public static List<AndroidDevice>  getAllDevices() {


            var result = Shell("adb", "devices").Result;

            var rts= new List<AndroidDevice>();

            foreach (Match mch in Regex.Matches(result, "\\n.*\\tdevice")) {
                string x = mch.Value;
                x = x.Substring(0, x.LastIndexOf("device")).Trim();

                AndroidDevice ad = new AndroidDevice(x);
                rts.Add(ad);
            }
            return rts;
        }






        /// <summary>
        /// 按下
        /// </summary>
        public void TouchDown(int X, int Y) {
            minitouch.TouchDown(X* scale, Y* scale);
        }

        /// <summary>
        /// 松开
        /// </summary>
        public void TouchUp() {
            minitouch.TouchUp();
        }

        /// <summary>
        /// 移动
        /// </summary>
        public void TouchMove(int X, int Y) {
            minitouch.TouchMove(X* scale, Y* scale);
        }


      

        private Process StartProcess(string fileName, string arguments) {
            
            var psi = new ProcessStartInfo(fileName, arguments);
            psi.RedirectStandardOutput = false;


            return Process.Start(psi);
        }


      

        /// <summary>
        /// push文件到手机
        /// </summary>
        /// <param name="localpath">文件地址</param>
        /// <param name="devicepath">手机位置</param>
        private void pushFile(string localpath, string devicepath) {
            Shell("adb", $"-s {deviceName} push {localpath} {devicepath}").Wait();
        }



        private async Task<string> adbByDevice(string arguments) {
            return await Shell("adb", $"-s {deviceName} {arguments}");
        }


        private static async Task<string> Shell(string fileName, string arguments) {

            return await Task.Run(() => {
                try {
                    var psi = new ProcessStartInfo(fileName, arguments);
                    psi.RedirectStandardOutput = true;

                    using (var process = Process.Start(psi)) {
                        string rt = process.StandardOutput.ReadToEnd();
                        Console.WriteLine(rt);
                        return rt;
                    }
                } catch (Exception e) {

                    return e.StackTrace; ;
                }

            });
        }

    }
}
