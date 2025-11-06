using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.Web.Security;

namespace _24DH11420_LTTH_BE234.Areas.Admin.Controllers
{
    public class HomeController : BaseController
    {
        string connectionString = @"Data Source=ASUS-TUF-F15\SQLEXPRESS; Initial Catalog=MyStore; Integrated Security=True; MultipleActiveResultSets=True";
        private MyStoreEntities db = new MyStoreEntities();
        public HomeController()
        {
            
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }
        

        

        public ActionResult Index()
        {

            var statistics = db.Products
        .GroupBy(p => p.Category.CategoryName) 
        .Select(g => new CategoryStatisticVM
        {
            CategoryName = g.Key, 
            ProductCount = g.Count(), 
            MinPrice = g.Min(p => p.ProductPrice), 
            MaxPrice = g.Max(p => p.ProductPrice), 
            AvgPrice = g.Average(p => p.ProductPrice)
        })
        .OrderBy(s => s.CategoryName)
        .ToList();

           
            var chartData = statistics.Select(s => new
            {
                CategoryName = s.CategoryName,
                ProductCount = s.ProductCount
            }).ToList();

            
            ViewBag.Statistics = statistics; 
            ViewBag.ChartData = new MvcHtmlString(Newtonsoft.Json.JsonConvert.SerializeObject(chartData)); 
           

            return View(); 
        }
        
        

        // GET: Admin/Home/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Admin/Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username.Trim()
                                      && u.Password == model.Password.Trim()
                                      && u.UserRole == "Admin");

                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    return RedirectToAction("Index");
                }
                else
                {
                    
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            
            return View(model);
        }

        // GET: Admin/Home/Logout
        public ActionResult Logout()
        {
            
            Session.Clear();

            
            FormsAuthentication.SignOut();

           
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}