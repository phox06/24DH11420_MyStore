using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
        [Authorize]
        public ActionResult Checkout()
        {
            var cart = GetCartService().GetCart();


            if (!cart.Items.Any())
            {

                return RedirectToAction("Index", "Home");
            }


            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {

                return RedirectToAction("Login", "Account");
            }


            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {

                return RedirectToAction("Login", "Account");
            }


            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                CustomerID = customer.CustomerID,
                Username = customer.Username,

                ShippingAddress = customer.CustomerAddress
            };

            return View(model);
        }
        [Authorize]
        public ActionResult MyOrder()
        {

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var orders = db.Orders
                .Where(o => o.CustomerID == customer.CustomerID)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // POST: Order/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartService().GetCart();

            if (!ModelState.IsValid)
            {

                model.CartItems = cart.Items.ToList();
                return View(model);
            }


            var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);


            if (model.PaymentMethod == "Paypal")
            {

                Session["CheckoutModel"] = model;
                return RedirectToAction("PaymentWithPaypal", "Paypal");
            }


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


            db.Orders.Add(order);
            db.SaveChanges();


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


            GetCartService().ClearCart();


            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }
        public ActionResult OrderSuccess(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var order = db.Orders.Include(o => o.OrderDetails.Select(d => d.Product))
                                 .SingleOrDefault(o => o.OrderID == id.Value);


            if (order == null)
            {
                return HttpNotFound();
            }


            if (order.Customer.Username != User.Identity.Name)
            {
                return HttpNotFound();
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