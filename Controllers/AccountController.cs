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
               
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "Customer"
                };
                db.Users.Add(user);

                
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                
                try
                {
                    db.SaveChanges(); 

                    
                    FormsAuthentication.SetAuthCookie(model.Username, false);

                    
                    return RedirectToAction("Index", "Home");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                   
                    var errorMessages = dbEx.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                    
                    var fullErrorMessage = string.Join("; ", errorMessages);

                    
                    var exceptionMessage = string.Concat("Lỗi validation: ", fullErrorMessage);

                    
                    throw new System.Data.Entity.Validation.DbEntityValidationException(exceptionMessage, dbEx.EntityValidationErrors);
                }
            }

            
            return View(model);
        }
        [Authorize] 
        public ActionResult ProfileInfo()
        {
            
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);

            if (customer == null)
            {
                
                FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }

            return View(customer); 
        }
        [Authorize] 
        public ActionResult UpdateProfile()
        {
           
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return HttpNotFound();
            }

            
            var model = new UpdateProfileVM
            {
                CustomerName = customer.CustomerName,
                CustomerEmail = customer.CustomerEmail,
                CustomerPhone = customer.CustomerPhone,
                CustomerAddress = customer.CustomerAddress
            };

            return View(model);
        }

       
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(UpdateProfileVM model)
        {
            if (!ModelState.IsValid)
            {
               
                return View(model);
            }

            
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);
            if (customer == null)
            {
                return HttpNotFound();
            }

            
            customer.CustomerName = model.CustomerName;
            customer.CustomerEmail = model.CustomerEmail;
            customer.CustomerPhone = model.CustomerPhone;
            customer.CustomerAddress = model.CustomerAddress;

            
            db.Entry(customer).State = EntityState.Modified;

            
            db.SaveChanges();

            
            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";

           
            return RedirectToAction("ProfileInfo");
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
               
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                      && u.Password == model.Password
                                                      && u.UserRole == "Customer");

                if (user != null)
                {
                   
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    
                    FormsAuthentication.SetAuthCookie(user.Username, false);
                    var cartService = new CartService(Session);
                    cartService.LoadCartFromDatabase(model.Username);


                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }
        [Authorize]
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
                
                return View(model);
            }

            
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return HttpNotFound();
            }

            
            if (user.Password != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "Mật khẩu cũ không chính xác.");
                return View(model);
            }

            user.Password = model.NewPassword;

            
            db.Entry(user).State = EntityState.Modified;

            
            db.SaveChanges();

            
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";

            
            return RedirectToAction("ProfileInfo");
        }
        // GET: Account/Logout
        public ActionResult Logout()
        {
            var cartService = new CartService(Session);
            if (User.Identity.IsAuthenticated)
            {
                cartService.SaveCartToDatabase(User.Identity.Name);
            }

            Session.Clear();

            
            FormsAuthentication.SignOut();


            return RedirectToAction("Login", "Account");
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