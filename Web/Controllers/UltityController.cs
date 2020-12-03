using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class UltityController : Controller
    {
        // GET: Ultity
        public ActionResult CountProduct(int count)
        {
            ViewBag.count = count;
            return View();
        }
        public ActionResult SelectFilter(int id,string listCheck)
        {
            ViewBag.listCheck = listCheck;
            ViewBag.id = id;
            return View();
        }
    }
}