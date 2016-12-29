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
            ViewBag.width = ad.width / ad.minitouchScale;
            ViewBag.height = ad.height / ad.minitouchScale;
            return View();
        }


        private static bool isControl;


        [Route("Websocket")]
        public async Task Websocket() {

            if (Request.HttpContext.WebSockets.IsWebSocketRequest) {


                var webSocket = await Request.HttpContext.WebSockets.AcceptWebSocketAsync();


               

                //添加图像输出事件
                var MinicapEvent = ad.AddMinicapEvent(delegate (byte[] imgByte) {
                    webSocket.SendAsync(new ArraySegment<byte>(imgByte), WebSocketMessageType.Binary, true, CancellationToken.None);
                });



                byte[] buffer = new byte[128];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0') == "control" && isControl == false) {
                    isControl = true;
                    ad.StartMinicapServer();
                    ad.StartMiniTouchServer();
                    //确保服务已经开启
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

                    ad.RemoveMinicapEvent(MinicapEvent);

                    ad.StopMinicap();
                    ad.StopMiniTouch();

                    isControl = false;

                } else {

                    await webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("确认操作者后会显示手机图像,请耐心等待!")), WebSocketMessageType.Text, true, CancellationToken.None);

                    while (true) {
                        byte[] ReceiveBuffer = new byte[128];
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(ReceiveBuffer), CancellationToken.None);

                        if (result.CloseStatus.HasValue) break;
                    }
                    ad.RemoveMinicapEvent(MinicapEvent);

                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                webSocket.Dispose();

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
                case "4":
                    ad.ClickKeycode(Convert.ToInt32(strArry[1]));
                    break;
                default:
                    break;
            }
        }


    }
}
