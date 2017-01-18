
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareDevice.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShareDevice.Controllers {
    public class HomeController : Controller {


        public IActionResult Index() {
            if(WSController.ad!=null) {
                ViewBag.CPU = WSController.ad.abi;
                ViewBag.SDK = WSController.ad.sdk;
                ViewBag.width = WSController.ad.width;
                ViewBag.height = WSController.ad.height;
                ViewBag.Model = WSController.ad.model;
            }

            var addr = Request.HttpContext.Connection.RemoteIpAddress.ToString();

            if (addr == "127.0.0.1" || addr == "::1") {
                ViewBag.Review = true;
            }


            return View();
        }

        [Route("Control")]
        public IActionResult Control() {

            if(WSController.ad == null) {
                return Content("没有可用设备");
            }

            if (WSController.isControl) {
                return View("ToWatch");
            } else {
                ViewBag.width = WSController.ad.width / WSController.ad.minitouchScale;
                ViewBag.height = WSController.ad.height / WSController.ad.minitouchScale;
                return View();
            }
            
        }

        [Route("Watch")]
        public IActionResult Watch() {

            if (WSController.ad == null) {
                return Content("没有可用设备");
            }

            ViewBag.width = WSController.ad.width / WSController.ad.minitouchScale;
            ViewBag.height = WSController.ad.height / WSController.ad.minitouchScale;
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

                

                return View(videoList);
            } else {
                return Content("您没有权限");
            }
        }

    }
}
