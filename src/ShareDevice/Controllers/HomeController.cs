
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareDevice.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShareDevice.Controllers {
    public class HomeController : Controller {


        public IActionResult Index() {
            if (SocketHandler.ad != null) {
                ViewBag.CPU = SocketHandler.ad.abi;
                ViewBag.SDK = SocketHandler.ad.sdk;
                ViewBag.width = SocketHandler.ad.width;
                ViewBag.height = SocketHandler.ad.height;
                ViewBag.Model = SocketHandler.ad.model;

                if (SocketHandler.ad.visable) {
                    ViewBag.Visable = true;
                }else {
                    ViewBag.Visable = false;
                }
            }

            var addr = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (addr == "127.0.0.1" || addr == "::1") {
                ViewBag.Review = true;
            }


            return View();
        }

        [Route("Control")]
        public IActionResult Control() {

            if (SocketHandler.ad == null || SocketHandler.ad.visable == false) {
                return Content("没有可用设备");
            }

            if (SocketHandler.isControl) {
                return View("ToWatch");
            } else {
                ViewBag.width = SocketHandler.ad.width / SocketHandler.ad.minitouchScale;
                ViewBag.height = SocketHandler.ad.height / SocketHandler.ad.minitouchScale;
                return View();
            }
            
        }

        [Route("Watch")]
        public IActionResult Watch() {

            if (SocketHandler.ad == null || SocketHandler.ad.visable == false) {
                return Content("没有可用设备");
            }

            ViewBag.width = SocketHandler.ad.width / SocketHandler.ad.minitouchScale;
            ViewBag.height = SocketHandler.ad.height / SocketHandler.ad.minitouchScale;
            return View();
        }




        [Route("ReView")]
        public IActionResult ReView() {
            //var ip = Request.Headers["X-Forwarded-For"]; //代理相关,无需考虑
            var addr = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (addr == "127.0.0.1" || addr == "::1") {
                //context.Succeed(requirement);

                DirectoryInfo TheFolder = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Replay"));

                List<ReViewModel> videoList = new List<ReViewModel>();
                //遍历文件
                foreach (FileInfo file in TheFolder.GetFiles()) {
                    if (file.Extension == ".zip" && file.Length > 0) {
                        videoList.Add(new ReViewModel { name = file.Name, length = file.Length, creationTime = file.CreationTime });
                    }
                }

                ViewBag.Path = TheFolder;

                return View(videoList);
            } else {
                return Content("您没有权限");
            }
        }

    }
}
