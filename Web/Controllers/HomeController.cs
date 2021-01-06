using Models;
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
            ViewBag.products = await db.Products.Where(x => x.Status == true).ToListAsync();
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
            var users = db.Customers.ToList();
            return PartialView("_TopMenu", users);
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

            var products = db.Products.Where(x => x.Status == true).OrderByDescending(x => x.ProductSaleQuantity).Take(3).ToList();
            var Providers = db.Providers.Where(x => x.Status == 1).ToList();
            ViewBag.products = products;
            ViewBag.Providers = Providers;
            return PartialView("Categories", db.Categories.Where(x => x.Status == 1).OrderBy(x => x.Orderby));
        }
        #region Product
        //GET: /Product by category
        public async Task<ActionResult> Products(int id = -1, string Sale = "default", string orderby = "default", string listProviderID = "-1", string listPriceID = "-1",string key="",int page = 1, int pageSize = 6)
        {
            var list = listProviderID.Split(',').Select(Int64.Parse).ToList();
            List<Product> products;
            if (id == -1)
            {
                ViewBag.listCheck = listProviderID;
                ViewBag.FilterPrice = listPriceID;
                var products1 = db.Products.Where(x => x.Status == true).ToList();
                //return HttpNotFound();
                products = new List<Product>();
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
                switch (listPriceID)
                {
                    case "1":
                        products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) < 100000).ToList();
                        break;
                    case "2":
                        products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 100000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 300000)).ToList();
                        break;
                    case "3":
                        products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 300000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 500000)).ToList();
                        break;
                    case "4":
                        products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) > 500000).ToList();
                        break;
                    default:
                        products = products.ToList();
                        break;
                }
            }
            else
            {
                ViewBag.Category = db.Categories.Where(x => x.CategoryId == id).FirstOrDefault();
                ViewBag.listCheck = listProviderID;
                ViewBag.FilterPrice = listPriceID;
                var productsAndCategories = (from c in db.Categories join p in db.Products on c.CategoryId equals p.CategoryId into table from p in table.DefaultIfEmpty() select new { p, c }).Where(x => (x.p.Status == true) && (x.p.CategoryId == id || x.c.ParentId == id)).OrderBy(x => x.p.CreateDate).ToList();
                var products1 = productsAndCategories.Select(x => x.p);
                products = new List<Product>();
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
            }
            if (Sale == "flashsale")
            {
                ViewBag.Sale = "Sale";
                products = products.Where(x => x.Discount != 0 && x.Status == true).OrderBy(p => p.CreateDate).ToList();
            }
            switch (listPriceID)
            {
                case "1":
                    products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) < 100000).ToList();
                    break;
                case "2":
                    products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 100000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 300000)).ToList();
                    break;
                case "3":
                    products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 300000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 500000)).ToList();
                    break;
                case "4":
                    products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) > 500000).ToList();
                    break;
                default:
                    products = products.ToList();
                    break;
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
                    products = products.OrderByDescending(p => p.CreateDate).ToList();
                    break;
                case "oldest_day":
                    ViewBag.date = "selected";
                    products = products.OrderBy(p => p.CreateDate).ToList();
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
            //tìm kiếm với key
            products = products.Where(x => (String.IsNullOrEmpty(key))||x.ProductName.ToLower().Contains(key.ToLower())).ToList();
            ViewBag.countProducts = products.ToList().Count();
            return View(products.ToPagedList(page, pageSize));
        }

        public async Task<ActionResult> ListProducts(int id = -1, string Sale = "default", string orderby = "default", string listProviderID = "-1", string listPriceID = "-1",string key="",int page = 1, int pageSize = 6)
        {
            var list = listProviderID.Split(',').Select(Int64.Parse).ToList();
            List<Product> products;
            if (id == -1)
            {
                ViewBag.listCheck = listProviderID;
                ViewBag.FilterPrice = listPriceID;
                var products1 = db.Products.Where(x => x.Status == true).ToList();
                //return HttpNotFound();
                products = new List<Product>();
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
                switch (listPriceID)
                {
                    case "1":
                        products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) < 100000).ToList();
                        break;
                    case "2":
                        products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 100000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 300000)).ToList();
                        break;
                    case "3":
                        products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 300000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 500000)).ToList();
                        break;
                    case "4":
                        products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) > 500000).ToList();
                        break;
                    default:
                        products = products.ToList();
                        break;
                }
            }
            else
            {
                ViewBag.Category = db.Categories.Where(x => x.CategoryId == id).FirstOrDefault();
                ViewBag.listCheck = listProviderID;
                ViewBag.FilterPrice = listPriceID;
                var productsAndCategories = (from c in db.Categories join p in db.Products on c.CategoryId equals p.CategoryId into table from p in table.DefaultIfEmpty() select new { p, c }).Where(x => (x.p.Status == true) && (x.p.CategoryId == id || x.c.ParentId == id)).OrderBy(x => x.p.CreateDate).ToList();
                var products1 = productsAndCategories.Select(x => x.p);
                products = new List<Product>();
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
            }
            if (Sale == "flashsale")
            {
                products = products.Where(x => x.Discount != 0 && x.Status == true).OrderBy(p => p.CreateDate).ToList();
            }
            switch (listPriceID)
            {
                case "1":
                    products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) < 100000).ToList();
                    break;
                case "2":
                    products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 100000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 300000)).ToList();
                    break;
                case "3":
                    products = products.Where(p => ((p.PriceOut - p.PriceOut * p.Discount / 100) >= 300000) && ((p.PriceOut - p.PriceOut * p.Discount / 100) < 500000)).ToList();
                    break;
                case "4":
                    products = products.Where(p => (p.PriceOut - p.PriceOut * p.Discount / 100) > 500000).ToList();
                    break;
                default:
                    products = products.ToList();
                    break;
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
                    products = products.OrderByDescending(p => p.CreateDate).ToList();
                    break;
                case "oldest_day":
                    ViewBag.date = "selected";
                    products = products.OrderBy(p => p.CreateDate).ToList();
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
            products = products.Where(x => (String.IsNullOrEmpty(key)) || x.ProductName.ToLower().Contains(key.ToLower())).ToList();
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
        public ActionResult CategoryProductDetail(int? idProductPresent)
        {

            var idCategoryProductPresent = db.Products.Where(x => x.ProductId == idProductPresent).Select(x => x.CategoryId).FirstOrDefault();
            var ProductRelationship = db.Products.Where(x => (x.Status == true) && (x.ProductId != idProductPresent) && (x.CategoryId == idCategoryProductPresent)).OrderByDescending(x => x.ProductSaleQuantity).Take(3).ToList();
            var products = db.Products.Where(x => x.Status == true).OrderByDescending(x => x.ProductSaleQuantity).Take(3).ToList();
            var ProductSale = db.Products.Where(x => x.Status == true && x.Discount > 0).OrderByDescending(x => x.Discount).Take(3).ToList();
            ViewBag.ProductSale = ProductSale;
            ViewBag.ProductRelationship = ProductRelationship;
            ViewBag.products = products;
            return View();
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
       
        #endregion

        #region News, New detail
        [AllowAnonymous]
        public ActionResult News(string q = "", int page = 1, int pageSize = 3)
        {
            var keysearch = q;
            keysearch = keysearch.Replace('+',' ');
            List<News> news = new List<News>();

            if (keysearch == "")
            {
                news = db.News.Where(n => n.Status == 1).OrderByDescending(n => n.Created).ToList();
            }
            else
            {
                news = db.News.Where(n => (n.Status == 1) && (n.NewsTitle.ToLower().Contains(keysearch.ToLower()))).OrderByDescending(n => n.Created).ToList();
            }
            var users = db.Users.Where(x => x.Status == 1).ToList();
            ViewBag.users = users;

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
            var news = db.News.Where(n => (n.Status == 1) && n.NewsId == id).SingleOrDefault();
            ViewBag.NewsRecent = db.News.Where(n => (n.Status == 1)&&n.NewsId!=id).OrderByDescending(x => x.Created).Take(3).ToList();
            var users = db.Users.Where(x => x.Status == 1).ToList();
            ViewBag.users = users;
            return View(news);
        }
        #endregion
        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(Contact contact)
        {

            if (ModelState.IsValid)
            {
                contact.Created = DateTime.Now;
                contact.Updated = DateTime.Now;
                db.Contacts.Add(contact);
                db.SaveChanges();

                return View();
            }
            else
            {
                return Json(new { Mesage = "flase" });
            }
        }
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