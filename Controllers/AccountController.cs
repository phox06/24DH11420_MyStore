using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using System.Web.Mvc;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.Web.Security;    

namespace _24DH11420_LTTH_BE234.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Nên mã hóa mật khẩu trong dự án thực tế
                    UserRole = "Customer"
                };
                db.Users.Add(user);

                // 2. Tạo bản ghi Customer
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                // 3. Lưu cả hai vào CSDL
                db.SaveChanges();

                // 4. Tự động đăng nhập người dùng sau khi đăng ký
                FormsAuthentication.SetAuthCookie(model.Username, false);

                // 5. Chuyển hướng về Trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Nếu có lỗi, hiển thị lại form
            return View(model);
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tài khoản đăng nhập (chỉ cho phép Customer đăng nhập ở đây)
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                      && u.Password == model.Password
                                                      && u.UserRole == "Customer");

                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    // Lưu thông tin xác thực người dùng vào Cookie
                    FormsAuthentication.SetAuthCookie(user.Username, false); // false = không lưu cookie vĩnh viễn

                    // Chuyển hướng về Trang chủ
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }
        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Xóa Session
            Session.Clear();

            // Xóa Cookie xác thực
            FormsAuthentication.SignOut();

            // Chuyển hướng về Trang chủ
            return RedirectToAction("Index", "Home");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Account
        public ActionResult Index()
        {

            return View();
        }
    }
}