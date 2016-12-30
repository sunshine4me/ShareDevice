using Microsoft.AspNetCore.Mvc;

namespace ShareDevice.Controllers {
    public class HomeController : Controller {


        public IActionResult Index() {
            ViewBag.CPU = WSController.ad.abi;
            ViewBag.SDK = WSController.ad.sdk;
            ViewBag.width = WSController.ad.width;
            ViewBag.height = WSController.ad.height;
            return View();
        }

        [Route("Control")]
        public IActionResult Control() {
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
            ViewBag.width = WSController.ad.width / WSController.ad.minitouchScale;
            ViewBag.height = WSController.ad.height / WSController.ad.minitouchScale;
            return View();
        }


    

        

    }
}
