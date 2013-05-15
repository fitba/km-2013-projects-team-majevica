using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FITkms.Models;

namespace FITkms.Controllers
{
    public class UserController : Controller
    {

        //
        // GET: /User/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(int broj = 5)
        {
            var model = new UserModel();
            var username = Request["txtusername"];
            var password = Request["txtpassword"];
            if (model.GetLogin(username, password) != null)
            {
                //dodaj u sesiju
                Response.Redirect("/");
            }
            else
            {
                Response.Redirect("/user/login");
            }
            return View();
        }

    }
}
