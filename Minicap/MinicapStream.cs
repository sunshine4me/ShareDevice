using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Minicap
{


    public delegate void MinicapEventHandler(byte[] imgByte);


    public class MinicapStream 
    {
        //定义push委托，用于写入图片流后通知其他监听器更新对象
        public event MinicapEventHandler push;

        //图像连接断开后的通知事件
        public event Action stopNotice;



        //定义IP和监听的端口
        private String IP = "127.0.0.1";

        public int PORT { get; private set; }


        //用于存放banner头信息
        private Banner banner = new Banner();
        private Socket minicapSocket;
        byte[] chunk = new byte[1024 * 10];




        /// <summary>
        /// minicap 接收信息的线程,想强制关闭时可用
        /// </summary>
        private Task ReadImageStreamTask;

        

        public MinicapStream() {
            PORT = 1313;
        }



        /// <summary>
        /// 开始执行
        /// </summary>
        public void Run() {
            //启动socket连接
            minicapSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            minicapSocket.Connect(new IPEndPoint(IPAddress.Parse(IP), PORT));
            

            ReadImageStreamTask = Task.Run(() => {
                try {
                    ReadImageStream();
                } catch (Exception e) {
                    Console.WriteLine("error : >>>>>>>>>>>>>> ReadImageStream");
                    Console.WriteLine(e.StackTrace);
                } finally {
                    minicapSocket.Dispose();
                }
                
                Console.WriteLine("info : >>>>>>>>>>>>>> 关闭minicap");
                //结束通知
                if (stopNotice != null) {
                    var events = stopNotice.GetInvocationList();
                    foreach (Action d in events) {
                        Task.Factory.StartNew(() => {
                            d();
                        }).ContinueWith(t => {
                            stopNotice -= d;
                        });
                    }
                }
                
            });
            
        }

        


       


        /// <summary>
        /// 用于提取byte数组
        /// </summary>
        /// <param name="arr">源数组</param>
        /// <param name="start">起始位置</param>
        /// <param name="end">结束位置</param>
        /// <returns>提取后的数组</returns>
        private byte[] subByteArray(byte[] arr, int start, int end)
        {
            int len = end - start;
            byte[] newbyte = new byte[len];
            Buffer.BlockCopy(arr, start, newbyte, 0, len);
            return newbyte;
        }

        /// <summary>
        /// 清理所有的事件
        /// </summary>
        public void clearPushEvent() {
            if(push!=null)
                foreach(var d in push.GetInvocationList()) {
                    push -= (d as MinicapEventHandler);
                }
        }


        /// <summary>
        /// 读取图片流到队列
        /// </summary>
        public void ReadImageStream()
        {
            int reallen;
            int readBannerBytes = 0;
            int bannerLength = 2;
            int readFrameBytes = 0;
            int frameBodyLength = 0;
            byte[] frameBody = new byte[0];
            while ((reallen = minicapSocket.Receive(chunk)) != 0)
            {
                
                for (int cursor = 0, len = reallen; cursor < len; )
                {
                    //读取banner信息
                    if (readBannerBytes < bannerLength)
                    {
                        switch (readBannerBytes)
                        {
                            case 0:
                                banner.Version = chunk[cursor];
                                break;
                            case 1:
                                banner.Length = bannerLength = chunk[cursor];
                                break;
                            case 2:
                            case 3:
                            case 4:
                            case 5:
                                banner.Pid += (chunk[cursor] << ((cursor - 2) * 8));
                                break;
                            case 6:
                            case 7:
                            case 8:
                            case 9:
                                banner.RealWidth += (chunk[cursor] << ((cursor - 6) * 8));
                                break;
                            case 10:
                            case 11:
                            case 12:
                            case 13:
                                banner.RealHeight += (chunk[cursor] << ((cursor - 10) * 8));
                                break;
                            case 14:
                            case 15:
                            case 16:
                            case 17:
                                banner.VirtualWidth += (chunk[cursor] << ((cursor - 14) * 8));
                                break;
                            case 18:
                            case 19:
                            case 20:
                            case 21:
                                banner.VirtualHeight += (chunk[cursor] << ((cursor - 2) * 8));
                                break;
                            case 22:
                                banner.Orientation += chunk[cursor] * 90;
                                //Console.WriteLine("Orientation:" + chunk[cursor]);
                                break;
                            case 23:
                                banner.Quirks = chunk[cursor];
                                break;
                        }
                        cursor += 1;
                        readBannerBytes += 1;
                    }
                    else if (readFrameBytes < 4)//读取每张图片的头4个字节(图片大小)
                    {
                        frameBodyLength += (chunk[cursor] << (readFrameBytes * 8));
                        cursor += 1;
                        readFrameBytes += 1;
                    }
                    else
                    {
                        //读取图片内容
                        if (len - cursor >= frameBodyLength)
                        {
                            
                            frameBody = frameBody.Concat(subByteArray(chunk, cursor, cursor + frameBodyLength)).ToArray();

                            //赋值内存,多线程
                            var tmp = (byte[])frameBody.Clone();
                            if (push != null) {
                                try {
                                    var events = push.GetInvocationList();
                                    foreach (MinicapEventHandler e in events) {
                                        Task.Factory.StartNew(() => {
                                            e(tmp);
                                        }).ContinueWith(t => {
                                            if (t.Status != TaskStatus.RanToCompletion) {
                                                push -= e;
                                                Console.WriteLine("error : push error!");
                                            }
                                        });
                                    }
                                } catch (Exception) {
                                    Console.WriteLine("error : push task error!");
                                }
                                
                            }
                               
                            cursor += frameBodyLength;
                            frameBodyLength = readFrameBytes = 0;
                            frameBody = new byte[0];
                        }
                        else
                        {
                            frameBody = frameBody.Concat(subByteArray(chunk, cursor, len)).ToArray();
                            frameBodyLength -= len - cursor;
                            readFrameBytes += len - cursor;
                            cursor = len;
                        }
                    }
                }
            }
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


       
    }
}
