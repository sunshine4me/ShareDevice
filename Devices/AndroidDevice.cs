using Minicap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Devices
{
    public class AndroidDevice {
        private MinicapStream minicap;

        public string deviceName { get; set; }

        public string MiniLibPath { get; set; }



        private int width;
        private int height;
        public int virtualwidth {  get; private set; }
        public int virtualheight { get; private set; }


        readonly private string GET_SIZE_COMMAND = "shell dumpsys window windows | grep mScreenRect";
        readonly private string GET_DEVICE_ABI_COMMAND = "shell getprop ro.product.cpu.abi";
        readonly private string GET_DEVICE_SDK_COMMAND = "shell getprop ro.build.version.sdk";





        public AndroidDevice(string _deviceName) {
            deviceName = _deviceName;

            var result = Shell("adb", GET_SIZE_COMMAND).Result;
            Match match = Regex.Match(result, @"\d{3,4}\,\d{3,4}");
            string size = match.Groups[0].Value;
            width = Convert.ToInt32(size.Split(',').ToArray()[0]);
            height = Convert.ToInt32(size.Split(',').ToArray()[1]);
            virtualwidth = width / 3;
            virtualheight = height / 3;

           
        }

        public MinicapEventHandler SetMinicapEvent {
            set {
                minicap.clearPushEvent();
                minicap.push += value;
            }
        }


        public void InitMinicap() {

            minicap = new MinicapStream();
            minicap.MINICAP_DEVICE_PATH = "/data/local/tmp";
            minicap.width = this.width;
            minicap.height = this.height;
            minicap.virtualwidth = this.virtualwidth;
            minicap.virtualheight = this.virtualheight;


            var abi = Shell("adb", GET_DEVICE_ABI_COMMAND).Result.Trim();
            var sdk = Shell("adb", GET_DEVICE_SDK_COMMAND).Result.Trim();

            var MINICAP_FILE_PATH = Path.Combine(MiniLibPath, $"minicap/bin/{abi}/minicap");
            var MINICAPSO_FILE_PATH = Path.Combine(MiniLibPath, $"minicap/shared/android-{sdk}/{abi}/minicap.so");

            pushFile(MINICAP_FILE_PATH, minicap.MINICAP_DEVICE_PATH);
            pushFile(MINICAPSO_FILE_PATH, minicap.MINICAP_DEVICE_PATH);

            Shell("adb", $"shell chmod 777 {minicap.MINICAP_DEVICE_PATH}/minicap").Wait();

        }

        public void startMinicap() {
            minicap.Run();
        }

        public void StopMinicap() {
            minicap.Stop();
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

        private static async Task<string> Shell(string fileName, string arguments) {

            return await Task.Run(() => {
                try {
                    var psi = new ProcessStartInfo(fileName, arguments);
                    psi.RedirectStandardOutput = true;

                    using (var process = Process.Start(psi)) {
                        return process.StandardOutput.ReadToEnd();
                    }
                } catch (Exception e) {

                    return e.StackTrace; ;
                }

            });
        }

        /// <summary>
        /// push文件到手机
        /// </summary>
        /// <param name="localpath">文件地址</param>
        /// <param name="devicepath">手机位置</param>
        private static void pushFile(string localpath, string devicepath) {
            Shell("adb", $"push {localpath} {devicepath}").Wait();
        }


    }
}
