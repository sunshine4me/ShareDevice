using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Devices;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Net.WebSockets;

namespace ShareDevice.Controllers {
    public class HomeController : Controller {
        public static AndroidDevice ad;

        public IActionResult Index() {
            ViewBag.width = ad.virtualwidth;
            ViewBag.height = ad.virtualheight;
            return View();
        }


        private static bool isClient;

        [Route("Websocket")]
        public async Task Websocket() {

            if (Request.HttpContext.WebSockets.IsWebSocketRequest) {

                //先1V1 不给多人连接,我觉得应该 lock 一下这个变量
                if (isClient) {
                    return;
                }

                ad.StartMinicapServer();
                ad.StartMiniTouchServer();

                var webSocket = await Request.HttpContext.WebSockets.AcceptWebSocketAsync();

                isClient = true;

                byte[] bufer = new byte[128];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(bufer), CancellationToken.None);

                

                ad.SetMinicapEvent = delegate (byte[] imgByte) {
                    webSocket.SendAsync(new ArraySegment<byte>(imgByte), WebSocketMessageType.Binary, true, CancellationToken.None);

                };


                Thread.Sleep(3000);

                

                ad.StartMinicap();

                ad.StartMiniTouch();

                await webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("已经连接手机,可以执行操作!")), WebSocketMessageType.Text, true, CancellationToken.None);



                while (true) {
                    byte[] ReceiveBuffer = new byte[128];
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(ReceiveBuffer), CancellationToken.None);

                    if (result.CloseStatus.HasValue) break;


                    TouchEvent(ReceiveBuffer);


                }

                ad.StopMinicap();
                ad.StopMiniTouch();

                isClient = false;


                Console.WriteLine("WebSocketCloseStatus>>>>>>>>>>>");
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                webSocket.Dispose();
                Console.WriteLine("Finished");

                

            } else {
                await Request.HttpContext.Response.WriteAsync("请使用Websocekt进行连接!");
            }
        }


        public IActionResult Error() {
            return View();
        }

        [NonAction]
        /// <summary>
        /// 屏幕操作
        /// </summary>
        /// <param name="buffer"></param>
        private void TouchEvent(byte[] buffer) {
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
                default:
                    break;
            }
        }


    }
}
