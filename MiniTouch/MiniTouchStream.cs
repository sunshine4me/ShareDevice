using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniTouch {
    public class MiniTouchStream {
        //定义IP和监听的端口
        private String IP = "127.0.0.1";

        public int PORT { get; private set; }
     
        private Socket minitouhSocket;

        private Banner Banner;



        public MiniTouchStream() {
            PORT = 1111;
        }


        public void Start() {
            

            minitouhSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            minitouhSocket.Connect(new IPEndPoint(IPAddress.Parse(IP), PORT));
            ParseBanner(minitouhSocket);
        }
        public void Stop() {

            try {
                minitouhSocket.Dispose();
            } catch (Exception) {

            }

         
        }




        /// <summary>
        /// 首次接收minitouh的banner信息
        /// </summary>
        /// <param name="socket"></param>
        private void ParseBanner(Socket socket) {
            byte[] chunk = new byte[64];
            socket.Receive(chunk);
            string[] result = Encoding.UTF8.GetString(chunk).Split(new char[2] { '\n', ' ' }).ToArray();
            Banner = new Banner();
            //读取banner数据
            Banner.Version = Convert.ToInt32(result[1]);
            Banner.MaxContacts = Convert.ToInt32(result[3]);
            Banner.MaxX = Convert.ToInt32(result[4]);
            Banner.MaxY = Convert.ToInt32(result[5]);
            Banner.MaxPressure = Convert.ToInt32(result[6]);
            Banner.Pid = Convert.ToInt32(result[8]);
            //换算真实设备和minitouch识别到支持的百分比

        }

        /// <summary>
        /// 按下操作
        /// </summary>
        public void TouchDown(int X,int Y) {
            //通过minitouch命令执行点击;传递的文本'd'为点击命令，0为触摸点索引，X Y 为具体的坐标值，50为压力值，注意必须以\n结尾，否则无法触发动作
            ExecuteTouch(string.Format("d 0 {0} {1} 50\n", X.ToString(), Y.ToString()));
        }

        public void TouchUp() {
            //松开触摸点
            ExecuteTouch(string.Format("u 0\n"));
        }

        public void TouchMove(int X, int Y) {
            //通过minitouch命令执行划动;传递的文本'd'为划动命令，0为触摸点索引，X Y 为要滑动到的坐标值，50为压力值，注意必须以\n结尾，否则无法触发动作
            ExecuteTouch(string.Format("m 0 {0} {1} 50\n", X.ToString(), Y.ToString()));
        }

        /// <summary>
        /// 发送定义好的触摸动作命令进行动作执行
        /// </summary>
        /// <param name="touchcommand">minitouch触摸命令</param>
        private void ExecuteTouch(string touchcommand) {
            //将动作数据转换为socket要提交的byte数据
            byte[] inbuff = Encoding.ASCII.GetBytes(touchcommand + "c\n");
            //发送socket数据
            minitouhSocket.Send(inbuff);
            /*
            //提交触摸动作的命令
            string ccommand = "c\n";
            inbuff = Encoding.ASCII.GetBytes(ccommand);
            //发送socket数据确认触摸动作的执行
            minitouhSocket.Send(inbuff);
            */
        }


        private async Task<string> Shell(string fileName, string arguments) {

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


        private Process StartProcess(string fileName, string arguments) {

            var psi = new ProcessStartInfo(fileName, arguments);
            psi.RedirectStandardOutput = true;


            return Process.Start(psi);
        }

    }


}
