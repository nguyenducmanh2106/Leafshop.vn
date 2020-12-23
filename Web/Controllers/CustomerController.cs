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
            var cookie = Request.Cookies["customer_token"];
            if (cookie != null)
            {
                return RedirectToAction("Index", "Home");
            }
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
            var user = db.Customers.Where(x => x.Email == customer.Email).FirstOrDefault();
            if (user != null)
            {
                return Json(new { result = false,message="Email đã tồn tại" });
            }
            customer.Password = GetMD5(customer.Password);
            customer.CreateDate = DateTime.Now;
            customer.Status = 1;
            customer.isEmailVerified = true;
            customer.ActiveCode = Guid.NewGuid();
            db.Customers.Add(customer);
            db.SaveChanges();

            return Json(new { result = true, message = "Đăng ký thành công" });
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
                    return RedirectToAction("Index", "Home");
                }

                else
                {
                    cookie.Value = isLogin.ActiveCode.ToString();
                    cookie.Expires = DateTime.Now.AddHours(1);
                    Response.Cookies.Add(cookie);
                    return RedirectToAction("Index", "Home");
                }
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
                ViewBag.Order = db.Orders.Where(x => x.CustomerId == user.CustomerId).ToList();
            }
           
            return View(user);
        }
        [HttpPost]
        public ActionResult ChangeProfile(Customer customer)
        {
            var user = db.Customers.Where(x => x.Email == customer.Email).FirstOrDefault();
            if (user == null)
            {
                return Json(new { result = false ,message="Cập nhật thông tin thất bại"});
            }
            user.Phone = customer.Phone;
            user.FullName = customer.FullName;
            user.Address = customer.Address;
            user.CreateDate = user.CreateDate;
            user.DateofBirth = customer.DateofBirth;
            user.Gender = customer.Gender;
            user.Status = 1;
            user.ActiveCode =user.ActiveCode;
            user.Tinh = customer.Tinh;
            user.Huyen = customer.Huyen;
            user.TinhId = customer.TinhId;
            user.HuyenId = customer.HuyenId;
            db.SaveChanges();
            return Json(new { result=true,message="Cập nhật thông tin thành công"});
        }
        public ActionResult Logout()
        {
            HttpCookie customer_token = Request.Cookies["customer_token"];
            customer_token.Path = "/";
            customer_token.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(customer_token);  
            return RedirectToAction("Index", "Customer");
        }
        public ActionResult Orders(string codeOrder)
        {
            var orders = db.Orders.Where(x => x.CodeOrder == codeOrder).SingleOrDefault();
            var orderDetail=db.OrderDetails.Where(x=>x.OrderId==orders.OrderId).ToList();
            var products = db.Products.ToList();
            ViewBag.orders = orders;
            ViewBag.orderDetail = orderDetail;
            ViewBag.products = products;
            return View();
        }
    }
}