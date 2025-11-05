using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _24DH11420_LTTH_BE234.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private CartService GetCartService()
        {
            return new CartService(Session);
        }
        [Authorize] // Bắt buộc người dùng phải đăng nhập
        public ActionResult Checkout()
        {
            var cart = GetCartService().GetCart();

            // Kiểm tra giỏ hàng
            if (!cart.Items.Any())
            {
                // Nếu giỏ hàng rỗng, chuyển về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Lấy thông tin người dùng đã đăng nhập
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                // Nếu không tìm thấy user (dù đã đăng nhập?), chuyển đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng từ CSDL
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                // Nếu không có thông tin khách hàng, chuyển đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            // Tạo đối tượng ViewModel
            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                CustomerID = customer.CustomerID,
                Username = customer.Username,
                // Lấy địa chỉ mặc định của khách hàng
                ShippingAddress = customer.CustomerAddress
            };

            return View(model);
        }
        [Authorize] // Bắt buộc người dùng phải đăng nhập
        public ActionResult MyOrder()
        {
            // Lấy thông tin người dùng đang đăng nhập
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tìm khách hàng tương ứng
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy tất cả đơn hàng của khách hàng này
            // Sắp xếp theo ngày mới nhất
            // Bao gồm cả Chi tiết đơn hàng (OrderDetails) và Sản phẩm (Product) liên quan
            var orders = db.Orders
                .Where(o => o.CustomerID == customer.CustomerID)
                .Include(o => o.OrderDetails.Select(d => d.Product)) // Tải thông tin Sản phẩm
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
        // POST: Order/Checkout
        // POST: Order/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartService().GetCart();

            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ, gửi lại CartItems
                model.CartItems = cart.Items.ToList();
                return View(model);
            }

            // Lấy thông tin khách hàng
            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);

            // ==================================================================
            // LOGIC MỚI: KIỂM TRA PHƯƠNG THỨC THANH TOÁN
            // ==================================================================
            if (model.PaymentMethod == "Paypal")
            {
                // Nếu là Paypal, lưu model checkout vào Session để PaypalController lấy
                // và chuyển hướng đến PaypalController
                Session["CheckoutModel"] = model;
                return RedirectToAction("PaymentWithPaypal", "Paypal");
            }

            // ==================================================================
            // LOGIC CŨ: XỬ LÝ CHO CÁC PHƯƠNG THỨC KHÁC (ví dụ: Tiền mặt)
            // ==================================================================

            // 1. Tạo đối tượng Order
            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = cart.TotalValue(),
                PaymentMethod = model.PaymentMethod,
                DeliveryMethod = model.DeliveryMethod,
                ShippingAddress = model.ShippingAddress,
                PaymentStatus = "Chưa thanh toán"
            };

            if (model.PaymentMethod == "Tiền mặt")
            {
                order.PaymentStatus = "Thanh toán tiền mặt";
            }

            // 2. Thêm Order vào CSDL
            db.Orders.Add(order);
            db.SaveChanges();

            // 3. Thêm các đối tượng OrderDetail
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                db.OrderDetails.Add(orderDetail);
            }

            db.SaveChanges();

            // 5. Xóa giỏ hàng
            GetCartService().ClearCart();

            // 6. Điều hướng đến trang xác nhận thành công
            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }
        public ActionResult OrderSuccess(int? id) // <-- BƯỚC 1: Thêm dấu '?' để cho phép 'id' có thể null (nullable int)
        {
            // BƯỚC 2: Kiểm tra xem 'id' có bị null không (ví dụ: truy cập URL trực tiếp)
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // BƯỚC 3: Dùng 'id.Value' để truy cập giá trị (vì chúng ta đã kiểm tra null)
            var order = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product)) // Thêm .Select(d => d.Product) để tải tên sản phẩm
                                 .SingleOrDefault(o => o.OrderID == id.Value);

            // BƯỚC 4: Tách logic kiểm tra ra
            if (order == null)
            {
                return HttpNotFound();
            }

            // Đảm bảo đơn hàng này thuộc về người dùng đang đăng nhập
            if (order.Customer.Username != User.Identity.Name)
            {
                return HttpNotFound(); // Hoặc bạn có thể trả về lỗi "Cấm truy cập"
            }

            return View(order);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Order
        public ActionResult Index()
        {
            return View();
        }
    }
}