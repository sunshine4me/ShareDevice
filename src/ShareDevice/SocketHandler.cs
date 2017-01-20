using Devices;
using ImageSharp;
using ImageSharp.Formats;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Minicap;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ShareDevice {
    public class SocketHandler {


        public static AndroidDevice ad;
        public static bool isControl;


        static async Task Control(HttpContext hc, Func<Task> n) {
            if (hc.WebSockets.IsWebSocketRequest) {
                if (isControl == false) {
                    isControl = true;
                    try {
                        await ControlDevice(hc);
                    } catch (Exception) {


                    } finally {
                        isControl = false;
                    }
                    isControl = false;
                } else {
                    await WatchDevice(hc);
                }
            }
        }


        static async Task Watch(HttpContext hc, Func<Task> n) {
            if (hc.WebSockets.IsWebSocketRequest) {
                await WatchDevice(hc);
            } 
        }

        static async Task WatchDevice(HttpContext hc) {

            using (var webSocket = await hc.WebSockets.AcceptWebSocketAsync()) {
                bool isPush = false;
                //添加图像输出事件
                var MinicapEvent = ad.AddMinicapEvent(delegate (byte[] imgByte) {
                    if (!isPush) {
                        isPush = true;
                        webSocket.SendAsync(new ArraySegment<byte>(imgByte), WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                        isPush = false;
                    }
                });
                //添加minicap 结束事件
                var MinicapStopNotice = ad.AddMinicapStopNotice(delegate () {
                    webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("图像输出已断开...")), WebSocketMessageType.Text, true, CancellationToken.None);
                });

                byte[] buffer = new byte[64];
                var result = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;


                webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("已经链接手机,请耐心等待图像传输!")), WebSocketMessageType.Text, true, CancellationToken.None).Wait();


                byte[] ReceiveBuffer = new byte[1024];
                var seg = new ArraySegment<byte>(ReceiveBuffer);
                while (webSocket.State == WebSocketState.Open) {
                    try {
                        result = await webSocket.ReceiveAsync(seg, CancellationToken.None);
                        if (result.CloseStatus.HasValue) {
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            break;
                        }
                    } catch (Exception) {
                        Console.WriteLine("error:webSocket ReceiveAsync");
                        break;
                    }
                }
                
                ad.RemoveMinicapEvent(MinicapEvent);
                ad.RemoveMinicapStopNotice(MinicapStopNotice);
            }


        }

        static async Task ControlDevice(HttpContext hc) {
            using (var webSocket = await hc.WebSockets.AcceptWebSocketAsync()) {
                bool isPush = false;
                
                //添加图像输出事件
                var MinicapEvent = ad.AddMinicapEvent(delegate (byte[] imgByte) {
                    if (!isPush) {
                        isPush = true;
                        webSocket.SendAsync(new ArraySegment<byte>(imgByte), WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                        isPush = false;
                    }
                });

                //添加minicap 结束事件
                var MinicapStopNotice =  ad.AddMinicapStopNotice(delegate () {
                    webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("图像输出已断开...")), WebSocketMessageType.Text, true, CancellationToken.None);
                    webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                });
                


                IImageEncoder imageEncoder = new JpegEncoder() {
                    Quality = 50,
                    Subsample = JpegSubsample.Ratio420
                };


                var vedio = ZipFile.Open(Path.Combine(Directory.GetCurrentDirectory(), $"Replay/{ad.model}-{ad.deviceName}-{DateTime.Now.ToString("yyyyMMddhhmmss")}.zip"), ZipArchiveMode.Create);
                bool isZipPush = false;
                DateTime lastImgDate = DateTime.Now;
                int imgCnt = 0;
                //添加录像事件
                var ZipEvent = ad.AddMinicapEvent(delegate (byte[] imgByte) {
                    if (!isZipPush) {
                        isZipPush = true;
                        if (imgCnt > 1500) {
                            vedio.Dispose();
                            vedio = ZipFile.Open(Path.Combine(Directory.GetCurrentDirectory(), $"Replay/{ad.model}-{ad.deviceName}-{DateTime.Now.ToString("yyyyMMddhhmmss")}.zip"), ZipArchiveMode.Create);
                            imgCnt = 0;
                        }

                        var nowDate = DateTime.Now;
                        if ((nowDate - lastImgDate).TotalMilliseconds > 200) {

                            lastImgDate = nowDate;

                            Image image = new Image(imgByte);

                            //毫秒时间戳
                            var epoch = (nowDate.ToUniversalTime().Ticks - 621355968000000000) / 10000;

                            // 添加jpg
                            var e = vedio.CreateEntry($"{epoch}.jpg", CompressionLevel.Optimal);
                            using (var stream = e.Open()) {
                                image.Save(stream, imageEncoder);
                            }
                            imgCnt++;
                        }
                        isZipPush = false;
                    }
                });



                //第一次通信 暂时不做处理
                byte[] buffer = new byte[64];
                var result = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;

                webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("已经连接手机,可以进行操控.")), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("如长时间未显示图像,请尝试点击屏幕或按下home键!")), WebSocketMessageType.Text, true, CancellationToken.None).Wait();


                byte[] ReceiveBuffer = new byte[1024];
                var seg = new ArraySegment<byte>(ReceiveBuffer);
                while (webSocket.State == WebSocketState.Open) {
                    try {
                        result = await webSocket.ReceiveAsync(seg, CancellationToken.None);
                        if (result.CloseStatus.HasValue) {
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                            break;
                        }
                        
                    } catch (Exception) {
                        Console.WriteLine("error:webSocket ReceiveAsync");
                        break;
                    }
                    try {
                        var outgoing = new ArraySegment<byte>(ReceiveBuffer, 0, result.Count);
                        TouchEvent(outgoing.ToArray());
                    } catch (Exception) {
                        Console.WriteLine("error:TouchEvent");
                    }
                    
                }
              
                ad.RemoveMinicapEvent(MinicapEvent);
                ad.RemoveMinicapEvent(ZipEvent);
                ad.RemoveMinicapStopNotice(MinicapStopNotice);
                vedio.Dispose();
                Console.WriteLine(">>>>>>>>>>>>>> end Recording");
            }


        }

        static void TouchEvent(byte[] buffer) {
            string str = System.Text.Encoding.UTF8.GetString(buffer);
            var strArry = str.Split(':');

            if (strArry.Length < 2) return;

            switch (strArry[0]) {
                case "3": {
                        var pnt = strArry[1].Split(',');
                        int X = (int)Convert.ToDouble(pnt[0]);
                        int Y = (int)Convert.ToDouble(pnt[1]);
                        ad.TouchMove(X, Y);
                    }
                    break;
                case "1": {
                        var pnt = strArry[1].Split(',');
                        int X = (int)Convert.ToDouble(pnt[0]);
                        int Y = (int)Convert.ToDouble(pnt[1]);
                        ad.TouchDown(X, Y);
                    }
                    break;
                case "2":
                    ad.TouchUp();
                    break;
                case "4":
                    ad.ClickKeycode(Convert.ToInt32(strArry[1]));
                    break;
                default:
                    break;
            }
        }


        public static void MapControl(IApplicationBuilder app) {
            app.Use(SocketHandler.Control);
        }

        public static void MapWatch(IApplicationBuilder app) {
            app.Use(SocketHandler.Watch);
        }

        
    }
}
