﻿using Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Models.Models.DataModels;
using Web.Areas.Admin.Models;
using PagedList;
using System.Web.Helpers;
using System.Threading.Tasks;
using Models.ViewModels;
using System.Collections.Generic;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        MobileShopContext db = new MobileShopContext();

        #region Home, MainMenu, Header
        //GET: /Home
        public async Task<ActionResult> Index()
        {
            //TempData["ReturnUrl"] = Request.Url.AbsoluteUri;
            ViewBag.News = await db.News.Where(x => x.Status == 1).Take(3).OrderBy(x => x.Created).ToListAsync();
            ViewBag.ProductSaleQuick = await db.Products.Where(x => x.Status == true).Take(8).OrderBy(x => x.ProductSaleQuantity).ToListAsync();
            ViewBag.banner = await db.Banners.Where(x => x.Status == 1).Take(6).OrderBy(x => x.Orderby).ToListAsync();
            ViewBag.ProductsNew = await db.Products.Where(x => x.Status == true).OrderBy(x => x.CreateDate).Take(8).ToListAsync();
            var providers = await db.Providers.Where(x => x.Status == 1).OrderBy(x => x.Orderby).Take(3).ToListAsync();
            ViewBag.SalePrice = await db.Products.Where(x => x.Status == true).OrderBy(x => (x.PriceOut - x.PriceOut * x.Discount / 100)).Take(8).ToListAsync();
            return View(providers);
        }
        public ActionResult TopMenu()
        {
            return PartialView("_TopMenu");
        }
        //Parital: //get Categories
        public PartialViewResult MainMenu()
        {
            //var Categories = db.Categories.Where(x => x.Status == 1).ToList();
            //ViewBag.Categories = Categories;
            return PartialView("_MainMenu", db.Categories.Where(x => x.Status == 1).OrderBy(x => x.Orderby));
        }
        public ActionResult PhuongThucVanChuyen()
        {
            return PartialView("PhuongThucVanChuyen");
        }
        public ActionResult Slider()
        {
            return PartialView("_Slide");
        }
        //Parital: /Cart
        public PartialViewResult Header()
        {
            var id = Request.Cookies["InfoCustomer"] != null ? Request.Cookies["InfoCustomer"]["id"] : "";
            ViewBag.checkCusomter = db.AddToCarts.Where(x => x.CustomerId.ToString() == id).FirstOrDefault();
            ViewBag.CountPrice = db.AddToCarts.Where(x => x.CustomerId.ToString() == id).ToList();
            return PartialView("_Header");
        }
        public ActionResult Footer()
        {
            return PartialView("Footer");
        }
        #endregion
        public ActionResult Categories()
        {

            var products = db.Products.Where(x => x.Status == true).OrderByDescending(x => x.CreateDate).Take(3).ToList();
            var Providers = db.Providers.Where(x => x.Status == 1).ToList();
            ViewBag.products = products;
            ViewBag.Providers = Providers;
            return PartialView("Categories", db.Categories.Where(x => x.Status == 1).OrderBy(x => x.Orderby));
        }
        #region Product
        //GET: /Product by category
        public async Task<ActionResult> Products(int? id, string orderby = "default", string listProviderID = "-1", int page = 1, int pageSize = 1)
        {
            var list = listProviderID.Split(',').Select(Int64.Parse).ToList();
            if (id == null)
            {
                return HttpNotFound();
            }

            ViewBag.Category = db.Categories.Where(x => x.CategoryId == id).FirstOrDefault();
            ViewBag.listCheck = listProviderID;
            var productsAndCategories = (from c in db.Categories join p in db.Products on c.CategoryId equals p.CategoryId into table from p in table.DefaultIfEmpty() select new { p, c }).Where(x => (x.p.Status == true) && (x.p.CategoryId == id || x.c.ParentId == id)).OrderBy(x => x.p.CreateDate).ToList();
            var products1 = productsAndCategories.Select(x => x.p);
            List<Product> products = new List<Product>();
            if (list[0] == -1)
            {
                products = products1.ToList();
            }
            else
            {
                foreach (var item in products1)
                {
                    foreach (var ProviderId in list)
                    {
                        if (item.ProviderId == ProviderId)
                        {
                            products.Add(item);
                        }

                    }
                }
            }

            switch (orderby)
            {
                case "price_asc":
                    ViewBag.price_asc = "selected";
                    products = products.OrderBy(p => p.PriceOut - p.PriceOut * p.Discount / 100).ToList();
                    break;
                case "price_desc":
                    ViewBag.price_desc = "selected";
                    products = products.OrderByDescending(p => p.PriceOut - p.PriceOut * p.Discount / 100).ToList();
                    break;
                case "name_asc":
                    ViewBag.price_asc = "selected";
                    products = products.OrderBy(p => p.ProductName).ToList();
                    break;
                case "name_desc":
                    ViewBag.price_desc = "selected";
                    products = products.OrderByDescending(p => p.ProductName).ToList();
                    break;
                case "recent_day":
                    ViewBag.date = "selected";
                    products = products.OrderBy(p => p.CreateDate).ToList();
                    break;
                case "oldest_day":
                    ViewBag.date = "selected";
                    products = products.OrderByDescending(p => p.CreateDate).ToList();
                    break;
                case "best_selling":
                    ViewBag.popularity = "selected";
                    products = products.OrderByDescending(p => p.ProductSaleQuantity).ToList();
                    break;
                case "default":
                    ViewBag.defaults = "selected";
                    products = products.OrderBy(p => p.ProductName).ToList();
                    break;
                default:
                    products = products.OrderBy(p => p.CreateDate).Where(p => p.CategoryId == id).ToList();
                    break;
            }
            ViewBag.countProducts = products.ToList().Count();
            return View(products.ToPagedList(page, pageSize));
        }

        public async Task<ActionResult> ListProducts(int? id, string orderby = "default", string listProviderID = "-1", int page = 1, int pageSize = 1)
        {
            var list = listProviderID.Split(',').Select(Int64.Parse).ToList();
            if (id == null)
            {
                return HttpNotFound();
            }

            ViewBag.Category = db.Categories.Where(x => x.CategoryId == id).FirstOrDefault();

            var productsAndCategories = (from c in db.Categories join p in db.Products on c.CategoryId equals p.CategoryId into table from p in table.DefaultIfEmpty() select new { p, c }).Where(x => (x.p.Status == true) && (x.p.CategoryId == id || x.c.ParentId == id)).OrderBy(x => x.p.CreateDate).ToList();
            var products1 = productsAndCategories.Select(x => x.p);
            List<Product> products = new List<Product>();
            if (list[0] == -1)
            {
                products = products1.ToList();
            }
            else
            {
                foreach (var item in products1)
                {
                    foreach(var ProviderId in list)
                    {
                        if (item.ProviderId == ProviderId)
                        {
                            products.Add(item);
                        }
                       
                    }
                }
            }

            switch (orderby)
            {
                case "price_asc":
                    ViewBag.price_asc = "selected";
                    products = products.OrderBy(p => p.PriceOut - p.PriceOut * p.Discount / 100).ToList();
                    break;
                case "price_desc":
                    ViewBag.price_desc = "selected";
                    products = products.OrderByDescending(p => p.PriceOut - p.PriceOut * p.Discount / 100).ToList();
                    break;
                case "name_asc":
                    ViewBag.price_asc = "selected";
                    products = products.OrderBy(p => p.ProductName).ToList();
                    break;
                case "name_desc":
                    ViewBag.price_desc = "selected";
                    products = products.OrderByDescending(p => p.ProductName).ToList();
                    break;
                case "recent_day":
                    ViewBag.date = "selected";
                    products = products.OrderBy(p => p.CreateDate).ToList();
                    break;
                case "oldest_day":
                    ViewBag.date = "selected";
                    products = products.OrderByDescending(p => p.CreateDate).ToList();
                    break;
                case "best_selling":
                    ViewBag.popularity = "selected";
                    products = products.OrderByDescending(p => p.ProductSaleQuantity).ToList();
                    break;
                case "default":
                    ViewBag.defaults = "selected";
                    products = products.OrderBy(p => p.ProductName).ToList();
                    break;
                default:
                    products = products.OrderBy(p => p.CreateDate).ToList();
                    break;
            }
            ViewBag.countProducts = products.ToList().Count();
            return View(products.ToPagedList(page, pageSize));
        }
        #endregion

        #region Provider
        //GET: /Providers by category
        public ActionResult Providers(int? id, string orderby)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            var products = from p in db.Products.Where(x => x.Status == true)
                           select p;
            switch (orderby)
            {
                case "price_asc":
                    ViewBag.price_asc = "selected";
                    products = products.OrderBy(p => p.PriceOut - p.PriceOut * p.Discount / 100).Where(p => p.ProviderId == id);
                    break;
                case "price_desc":
                    ViewBag.price_desc = "selected";
                    products = products.OrderByDescending(p => p.PriceOut - p.PriceOut * p.Discount / 100).Where(p => p.ProviderId == id);
                    break;
                case "date":
                    ViewBag.date = "selected";
                    products = products.OrderBy(p => p.CreateDate).Where(p => p.ProviderId == id);
                    break;
                case "popularity":
                    ViewBag.popularity = "selected";
                    products = products.OrderByDescending(p => p.ProductSaleQuantity).Where(p => p.ProviderId == id);
                    break;
                case "default":
                    ViewBag.defaults = "selected";
                    products = products.OrderBy(p => p.CreateDate).Where(p => p.ProviderId == id);
                    break;
                default:
                    products = products.OrderBy(p => p.CreateDate).Where(p => p.ProviderId == id);
                    break;
            }
            return View(products);
        }
        #endregion
        public ActionResult QuickAddCart(int? id)
        {
            var product = db.Products.Where(x => x.Status == true && x.ProductId == id).FirstOrDefault();
            ViewBag.product = product;
            return PartialView("QuickAddCart");
        }
        #region Product detail, and AddToCart
        //GET: /Product detail
        [AllowAnonymous]
        public ActionResult Productsdetail(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = db.Products.Include(x => x.Categories).Include(x => x.ProductAttrs).Where(x => x.Status == true).FirstOrDefault(x => x.ProductId == id);
            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var data = (from p in db.ProductAttrs
                        join a in db.Attributes
                        on p.AttrId equals a.AttrId
                        join t in db.TypeAttrs on a.TypeId
                        equals t.TypeId
                        where p.ProductId == id
                        select new TypeAndAttr()
                        {
                            tenLoai = t.TypeName,
                            tenThuocTinh = a.AttrName
                        }
                               ).Distinct();
            ViewBag.TypeAttr = data.ToList();
            ViewBag.Providers = db.Providers.ToList();
            return View(product);

        }
        //POST: /Product detail -> add product to card
        [HttpPost]
        [CustomersAutherize]
        [ValidateAntiForgeryToken]
        public ActionResult Productsdetail(int? id, int quantity)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //Tìm sản phẩm cần mua
            var product = db.Products.Include(x => x.Categories).Include(x => x.ProductAttrs).Where(x => x.Status == true).FirstOrDefault(x => x.ProductId == id);
            if (product == null)
            {
                return RedirectToAction("Index", "Home");
            }
            //Lấy typeattr của sản phẩm
            var typesAttr = (from p in product.ProductAttrs
                             join a in db.Attributes
                             on p.AttrId equals a.AttrId
                             join t in db.TypeAttrs on a.TypeId
                             equals t.TypeId
                             select t
                                ).Distinct();
            string attrId = "";
            //Lấy các thuộc tính
            foreach (var item in typesAttr)
            {
                attrId = attrId + Request["attribute_" + item.TypeId].ToString() + ";";
            }
            //Lấy thông tin người dùng hiện tại 
            HttpCookie cookie = Request.Cookies["InfoCustomer"];
            var userId = cookie["id"];
            var parseIntUser = int.Parse(userId);
            var checkCustomer = db.Customers.Where(x => x.CustomerId == parseIntUser && x.Status != 10).SingleOrDefault();
            if (checkCustomer == null)
            {
                Response.Cookies["InfoCustomer"].Expires = DateTime.Now.AddDays(-2);
                Response.Cookies["Avatar"].Expires = DateTime.Now.AddDays(-2);
                return RedirectToAction("Login", "Users");
            }
            //Thêm mới sản phẩm sao giỏ hàng
            AddToCart atc = new AddToCart();
            atc.product = product;
            var getAttr = !String.IsNullOrEmpty(attrId) ?
                attrId = attrId.Substring(0, attrId.Length - 1) : "";
            atc.AttrId = getAttr;
            //convert String attrId to int 
            if (getAttr.Length > 0)
            {
                int intAttrId = int.Parse(getAttr);
                var findAttrId = db.Attributes.Where(x => x.Status == 1 && x.AttrId == intAttrId).FirstOrDefault();
                var attrName = findAttrId != null ? findAttrId.AttrName : "";
                atc.AttrName = attrName;
            }
            //getattrName from attrId
            //tính giá sản phẩm
            var priceOut = atc.product.PriceOut;
            var price = priceOut - priceOut * atc.product.Discount / 100;
            var discount = atc.product.Discount > 0 ? price : priceOut;
            atc.Price = (double)discount;
            atc.Quantity = quantity;
            atc.CustomerId = int.Parse(userId);
            //Kiểm trả giỏ hàng của người dùng hiện tại đã có sản phẩm hay chưa
            var cart = db.AddToCarts.Any();
            if (cart)
            {
                //Lấy tất cả sản phẩm hiện tại của customer hiện tại
                var listCart = db.AddToCarts.Where(x => x.CustomerId == parseIntUser).ToList();
                bool check = false;
                foreach (var item in listCart)
                {
                    //Kiểm tra sản phẩm hiện tại đã có trong giỏ hàng hay chưa.
                    if (item.product.ProductId == id && item.AttrId == attrId)
                    {
                        check = true;
                        //Cập nhập số lượng
                        item.Quantity += atc.Quantity;
                        db.SaveChanges();
                    }
                }
                //Nếu không sản phẩm hiện tại không trùng
                if (!check)
                {
                    db.AddToCarts.Add(atc);
                    db.SaveChanges();
                }
            }
            else
            {
                db.AddToCarts.Add(atc);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Cart");
        }
        #endregion

        #region News, New detail
        [AllowAnonymous]
        public ActionResult News(int page = 1, int pageSize = 9)
        {
            var news = db.News.Where(n => n.Status == 1).OrderByDescending(n => n.Created).ToList();
            return View(news.ToPagedList(page, pageSize));
        }
        [AllowAnonymous]
        [ValidateInput(false)]
        public ActionResult NewsDetail(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("News", "Home");
            }
            News news = db.News.Where(n => n.NewsId == id).FirstOrDefault();
            return View(news);
        }
        #endregion

        #region Feedback
        public ActionResult Feedback()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Feedback(Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentAvatar = Request.Cookies["InfoCustomer"] != null ? Request.Cookies["InfoCustomer"]["Avatar"] : "";
                    feedback.Created = DateTime.Now;
                    feedback.Status = 0;
                    feedback.Avatar = currentAvatar;
                    db.Feedbacks.Add(feedback);
                    db.SaveChanges();
                    ViewBag.message = "Liên hệ của bạn đã được gửi.";
                }
                catch (Exception)
                {
                    ViewBag.error = "Không thể gửi phản, vui lòng thử lại sau";
                    return View(feedback);
                }
            }
            return View(feedback);
        }
        #endregion

        #region Other
        public ActionResult DieuKhoanSuDung()
        {
            return PartialView("DieuKhoanSuDung");
        }
        public ActionResult ChinhSachDoiTra()
        {
            return PartialView("ChinhSachDoiTra");
        }
        public ActionResult Introduce()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public PartialViewResult Mainleft()
        {
            ViewBag.Provider = db.Providers.Where(x => x.Status == 1).OrderBy(x => x.Orderby).Take(6);
            ViewBag.News = db.News.Where(n => n.Status == 1).OrderByDescending(n => n.Created).Take(2).ToList();
            var productSalePrice = db.Products.Where(x => x.Status == true).OrderByDescending(x => x.ProductSaleQuantity).Take(4);
            ViewBag.ProductSaleQuantity = productSalePrice;
            return PartialView("_Mainleft", productSalePrice);
        }
        #endregion
    }
}