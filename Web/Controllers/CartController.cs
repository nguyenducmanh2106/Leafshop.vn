using Models;
using Models.Models.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Areas.Admin.Models;
using Models.ViewModels;
namespace Web.Controllers
{
    //[CustomersAutherize]
    public class CartController : Controller
    {
        public string cart_token = "cart_token";
        MobileShopContext db = new MobileShopContext();

        public ActionResult ToastrProduct(string productId)
        {
            int id = Int32.Parse(productId);
            var products = db.Products.Where(x => x.ProductId == id).FirstOrDefault();
            return View(products);
        }
        public ActionResult TotalProduct()
        {
            var CookieID = Request.Cookies[cart_token];
            int countProductInCart = 0;
            if (CookieID != null && CookieID.Value != "")
            {
                var productsID = CookieID.Value.Split('-').Select(x => Int32.Parse(x)).ToList();
                countProductInCart = productsID.Distinct().Count();

            }
            ViewBag.countProductInCart = countProductInCart;
            return View();
        }
        public ActionResult ViewQuick()
        {

            Cart cart = new Cart();
            var CookieID = Request.Cookies[cart_token];
            List<Product> listProduct = new List<Product>();
            if (CookieID != null && CookieID.Value != "")
            {
                var productsID = CookieID.Value.Split('-').Select(x => Int32.Parse(x)).ToList();
                var productsIdDistinct = productsID.Distinct();
                foreach (var item in productsIdDistinct)
                {
                    var prod = db.Products.Where(x => x.ProductId == item).FirstOrDefault();
                    listProduct.Add(prod);
                }
                cart.products = listProduct;
                cart.productsId = productsID;
            }
            return View(cart);
        }
        #region Carts
        // GET: Cart
        public ActionResult Index()
        {

            Cart cart = new Cart();
            var CookieID = Request.Cookies[cart_token];
            List<Product> listProduct = new List<Product>();
            if (CookieID != null && CookieID.Value != "")
            {
                var productsID = CookieID.Value.Split('-').Select(x => Int32.Parse(x)).ToList();
                var productsIdDistinct = productsID.Distinct();
                foreach (var item in productsIdDistinct)
                {
                    var prod = db.Products.Where(x => x.ProductId == item).FirstOrDefault();
                    listProduct.Add(prod);
                }
                cart.products = listProduct;
                cart.productsId = productsID;

            }

            return View(cart);
        }

        #endregion


        #region Payment
        //POST: Payment
        public ActionResult Payment()
        {
            //get id current User
            HttpCookie cookie = Request.Cookies["customer_token"];
            //get info current User
            Customer currentUser = null;
            if (cookie != null && cookie.Value != null)
            {
                currentUser = db.Customers.Where(x => x.ActiveCode.ToString() == cookie.Value).SingleOrDefault();
            }

            //check cart is empty
            Cart cart = new Cart();
            var CookieID = Request.Cookies[cart_token];
            List<Product> listProduct = new List<Product>();
            if (CookieID != null && CookieID.Value != "")
            {
                var productsID = CookieID.Value.Split('-').Select(x => Int32.Parse(x)).ToList();
                var productsIdDistinct = productsID.Distinct();
                foreach (var item in productsIdDistinct)
                {
                    var prod = db.Products.Where(x => x.ProductId == item).FirstOrDefault();
                    listProduct.Add(prod);
                }
                cart.products = listProduct;
                cart.productsId = productsID;
            }
            ViewBag.currentUser = currentUser;
            ViewBag.cart = cart;
            return View();
        }

        [HttpPost]
        public ActionResult Payment(string orderObj, string cart)
        {
            try
            {
                orderObj = orderObj.Replace("\\\"", "");
                Order order =
                    JsonConvert.DeserializeObject<Order>(orderObj);

                Random random = new Random(DateTime.Now.Ticks.GetHashCode());
                var getYear = DateTime.Now.Year.ToString();
                getYear = getYear.Substring(2);
                var randomCode = random.Next(100000, 999999).ToString() + getYear;
                var codeOrder = db.Orders.Any(x => x.CodeOrder.Equals(randomCode));
                while (codeOrder)
                {
                    randomCode = random.Next(100000, 999999).ToString() + getYear;
                }
                var customerId = db.Customers.Where(x => x.Email == order.Email).SingleOrDefault();
                Order orderTable = new Order()
                {
                    Created = DateTime.Now,
                    TimeExpires = DateTime.Now.AddMinutes(10),
                    Status = 0,
                    CodeOrder = randomCode,
                    CustomerId = customerId == null ? -1 : customerId.CustomerId,
                    FullName = order.FullName,
                    Email = order.Email,
                    Phone = order.Phone,
                    totalPrice = order.totalPrice,
                    City = order.City,
                    District = order.District,
                    Address = order.Address,

                };
                //thêm đơn hàng vào bảng order
                db.Orders.Add(orderTable);
                db.SaveChanges();
                var productsID = cart.Split('-').Select(x => Int32.Parse(x)).ToList();
                List<OrderDetail> arr = new List<OrderDetail>();
                foreach (var item in productsID.Distinct())
                {
                    var product = db.Products.Where(x => x.ProductId == item).SingleOrDefault();
                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderId = orderTable.OrderId,
                        ProductId = item,
                        Quantity = productsID.Where(x => x == item).Count(),
                        PriceOut = product.PriceOut,
                        Discount = product.Discount,
                        Price = (product.PriceOut * (1 - (float)product.Discount / 100)) * productsID.Where(x => x == item).Count()
                    };
                    db.OrderDetails.Add(orderDetail);
                    db.SaveChanges();
                }
                return Json(new { result = true, message = "Đặt hàng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = "Đặt hàng thất bại" });
            }
        }
        #endregion
    }
}