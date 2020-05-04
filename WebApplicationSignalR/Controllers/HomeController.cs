using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplicationSignalR.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Enviar(string mensagem)
        {
            HubRobo.HoradeTrabalhar(mensagem);

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
    }
}