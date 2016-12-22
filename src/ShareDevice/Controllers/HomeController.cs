using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Devices;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Net.WebSockets;

namespace ShareDevice.Controllers
{
    public class HomeController : Controller
    {
        public static AndroidDevice ad;

        public IActionResult Index()
        {
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


                var webSocket = await Request.HttpContext.WebSockets.AcceptWebSocketAsync();

                isClient = true;

                byte[] buffer = new byte[128];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                
                ad.SetMinicapEvent = delegate (byte[] imgByte) {
                    webSocket.SendAsync(new ArraySegment<byte>(imgByte), WebSocketMessageType.Binary, true, CancellationToken.None);

                };


                ad.startMinicap();


                while (!result.CloseStatus.HasValue) {

                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                }

                ad.StopMinicap();

                isClient = false;


                Console.WriteLine("WebSocketCloseStatus>>>>>>>>>>>");
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);

                webSocket.Dispose();
                Console.WriteLine("Finished");



            } else {
                await Request.HttpContext.Response.WriteAsync("请使用Websocekt进行连接!");
            }
        }


        public IActionResult Error()
        {
            return View();
        }
    }
}
