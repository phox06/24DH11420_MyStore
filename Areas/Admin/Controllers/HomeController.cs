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
            // Bật tính năng Log. 
            // Dòng này bảo EF: "Hãy in BẤT KỲ SQL nào bạn chạy ra cửa sổ Debug"
            db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }
        // ------------------------------------

        // GET: Admin/Home/Index (Đây là Dashboard)

        public ActionResult Index()
        {

            var statistics = db.Products
        .GroupBy(p => p.Category.CategoryName) // Nhóm theo Tên Danh mục
        .Select(g => new CategoryStatisticVM
        {
            CategoryName = g.Key, // Tên danh mục
            ProductCount = g.Count(), // Số lượng sản phẩm
            MinPrice = g.Min(p => p.ProductPrice), // Giá thấp nhất
            MaxPrice = g.Max(p => p.ProductPrice), // Giá cao nhất
            AvgPrice = g.Average(p => p.ProductPrice) // Giá trung bình
        })
        .OrderBy(s => s.CategoryName)
        .ToList();

            // 2. Chuẩn bị dữ liệu cho Google Chart
            // Chuyển đổi dữ liệu thống kê thành một mảng JSON
            var chartData = statistics.Select(s => new
            {
                CategoryName = s.CategoryName,
                ProductCount = s.ProductCount
            }).ToList();

            // 3. Truyền cả hai danh sách đến View
            ViewBag.Statistics = statistics; // Dùng cho bảng
            ViewBag.ChartData = new MvcHtmlString(Newtonsoft.Json.JsonConvert.SerializeObject(chartData)); // Dùng cho biểu đồ

            // ======== KẾT THÚC LOGIC THỐNG KÊ ========

            return View(); // <-- SỬA DÒNG NÀY THÀNH "return View();"
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
                // ĐẢM BẢO DÒNG NÀY ĐANG KIỂM TRA CHÍNH XÁC "Admin" (viết hoa)
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username.Trim()
                                      && u.Password == model.Password.Trim()
                                      && u.UserRole == "Admin");

                if (user != null)
                {
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole; // Dòng này rất quan trọng
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    return RedirectToAction("Index");
                }
                else
                {
                    // Dòng này sẽ chạy nếu không tìm thấy user
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            // Dòng này chạy nếu Model không hợp lệ
            return View(model);
        }

        // GET: Admin/Home/Logout
        public ActionResult Logout()
        {
            // Xóa Session
            Session.Clear();

            // Xóa Cookie xác thực
            FormsAuthentication.SignOut();

            // Chuyển hướng về trang Đăng nhập Admin
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