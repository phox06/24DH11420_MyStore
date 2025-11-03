using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.Data.Entity;
using PagedList;

namespace _24DH11420_LTTH_BE234.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        private CartService GetCartService()
        {
            return new CartService(Session);
        }
        // GET: Cart
        public ActionResult Index()
        {
            var cart = GetCartService().GetCart();
            return View(cart);
        }
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var cartService = GetCartService();
            var cart = cartService.GetCart();
            cart.AddItem(
                product.ProductID,
                product.ProductName,
                quantity,
                product.ProductPrice,
                product.ProductImage,
                product.Category.CategoryName
            );

            // Quay lại trang chi tiết sản phẩm
            return RedirectToAction("ProductDetails", "Home", new { id = id });
        }
        public ActionResult RemoveFromCart(int id)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();
            cart.RemoveItem(id);
            return RedirectToAction("Index");
        }

        // GET: Cart/ClearCart
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();
            cart.UpdateQuantity(id, quantity);
            return RedirectToAction("Index");
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