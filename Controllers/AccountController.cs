using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;   
using System.Data.Entity;

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

                // 1. Tạo bản ghi User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
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

                // 3. Bọc SaveChanges() trong try-catch để bắt lỗi validation
                try
                {
                    db.SaveChanges(); // <-- LỖI XẢY RA Ở ĐÂY

                    // 4. Tự động đăng nhập người dùng sau khi đăng ký
                    FormsAuthentication.SetAuthCookie(model.Username, false);

                    // 5. Chuyển hướng về Trang chủ
                    return RedirectToAction("Index", "Home");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    // Lấy tất cả các lỗi validation
                    var errorMessages = dbEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                    // Nối các lỗi lại thành một chuỗi
                    var fullErrorMessage = string.Join("; ", errorMessages);

                    // Tạo một chuỗi lỗi dễ đọc hơn
                    var exceptionMessage = string.Concat("Lỗi validation: ", fullErrorMessage);

                    // Ném lại lỗi với thông tin chi tiết hơn để dễ debug
                    throw new System.Data.Entity.Validation.DbEntityValidationException(exceptionMessage, dbEx.EntityValidationErrors);
                }
            }

            // Nếu có lỗi, hiển thị lại form
            return View(model);
        }
        [Authorize] // Bắt buộc người dùng phải đăng nhập
        public ActionResult ProfileInfo()
        {
            // Lấy thông tin khách hàng từ CSDL
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);

            if (customer == null)
            {
                // Nếu không tìm thấy thông tin khách hàng (lỗi lạ), bắt đăng xuất
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            return View(customer); // Truyền model Customer vào View
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
        [Authorize] // Bắt buộc người dùng phải đăng nhập
        public ActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ, hiển thị lại form
                return View(model);
            }

            // Lấy thông tin người dùng từ CSDL
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra xem mật khẩu cũ có đúng không
            if (user.Password != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            // Cập nhật mật khẩu mới
            // (Trong một dự án thực tế, bạn nên mã hóa (hash) mật khẩu này trước khi lưu)
            user.Password = model.NewPassword;

            // Đánh dấu là đã sửa đổi
            db.Entry(user).State = EntityState.Modified;

            // Lưu thay đổi
            db.SaveChanges();

            // Gửi một thông báo thành công tạm thời
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            // Chuyển hướng về trang thông tin tài khoản
            return RedirectToAction("ProfileInfo");
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