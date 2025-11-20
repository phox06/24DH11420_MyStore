using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models; // Ensure you have this using
using _24DH11420_LTTH_BE234.Models.ViewModel;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class CartService
    {
        private readonly HttpSessionStateBase session;
        private MyStoreEntities db = new MyStoreEntities(); // Add Database Context

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public Cart GetCart()
        {
            var cart = (Cart)session["Cart"];
            if (cart == null)
            {
                cart = new Cart();
                session["Cart"] = cart;
            }
            return cart;
        }

        public void ClearCart()
        {
            session["Cart"] = null;
        }

        // --- NEW METHOD: Save Session Cart to Database ---
        public void SaveCartToDatabase(string username)
        {
            var cart = GetCart();

            // 1. Clear old items for this user to avoid duplicates
            var oldItems = db.CartItems.Where(c => c.Username == username).ToList(); // Assuming you named the table CartItems in EDMX
            db.CartItems.RemoveRange(oldItems);
            db.SaveChanges();

            // 2. Add current session items to DB
            foreach (var item in cart.Items)
            {
                var dbItem = new _24DH11420_LTTH_BE234.Models.CartItem // Refers to the DB Entity, not ViewModel
                {
                    Username = username,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity
                };
                db.CartItems.Add(dbItem);
            }
            db.SaveChanges();
        }

        // --- NEW METHOD: Load Database Cart into Session ---
        public void LoadCartFromDatabase(string username)
        {
            var cart = GetCart(); // Get current session cart (might be empty or have guest items)

            var dbItems = db.CartItems.Where(c => c.Username == username).ToList();

            foreach (var dbItem in dbItems)
            {
                // We need product details (Name, Price, Image) which are not in CartItem table
                var product = db.Products.Find(dbItem.ProductID);
                if (product != null)
                {
                    // Reuse your existing AddItem logic to handle merging/totals
                    cart.AddItem(
                        product.ProductID,
                        product.ProductName,
                        dbItem.Quantity,
                        product.ProductPrice,
                        product.ProductImage,
                        product.Category.CategoryName
                    );
                }
            }

            // Update the session
            session["Cart"] = cart;
        }
    }
}