using Models;
using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class CustomerController : Controller
    {
        MobileShopContext db = new MobileShopContext();
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }
        public static string GetMD5(string chuoi)
        {
            string str_md5 = "";
            byte[] mang = System.Text.Encoding.UTF8.GetBytes(chuoi);

            MD5CryptoServiceProvider my_md5 = new MD5CryptoServiceProvider();
            mang = my_md5.ComputeHash(mang);

            foreach (byte b in mang)
            {
                str_md5 += b.ToString("X2");
            }

            return str_md5;
        }
        
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Register(Customer customer)
        {
            customer.Password = GetMD5(customer.Password);
            customer.CreateDate = DateTime.Now;
           
            customer.Status = 1;
            customer.isEmailVerified = true;
            customer.ActiveCode = Guid.NewGuid();
            db.Customers.Add(customer);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        //
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string Password)
        {

            string encodePassword = GetMD5(Password);
            var isLogin = db.Customers.Where(x => x.Email == Email && x.Password == encodePassword).FirstOrDefault();
            if (isLogin != null)
            {
                var cookie = Request.Cookies["customer_token"];
                if (cookie == null)
                {
                    HttpCookie customer_token = new HttpCookie("customer_token");
                    customer_token.Value = isLogin.ActiveCode.ToString();
                    customer_token.Expires = DateTime.Now.AddHours(1);
                    Response.Cookies.Add(customer_token);
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ms = "fail";
            return View("Index");
        }
        public ActionResult Profile()
        {
            var customer_token = Request.Cookies["customer_token"];
            Customer user = null;
            if (customer_token != null && customer_token.Value!="")
            {
                user = db.Customers.Where(x => x.ActiveCode.ToString() == customer_token.Value).FirstOrDefault();
            }
            return View(user);
        }
    }
}