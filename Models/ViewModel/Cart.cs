using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.Web.Mvc;
using PagedList;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items => items;

        // Thêm sản phẩm vào giỏ
        public void AddItem(int productId, string productName, int quantity, decimal unitPrice, string productImage, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);

            if (existingItem == null)
            {
                // Nếu sản phẩm chưa có trong giỏ
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductName = productName,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    ProductImage = productImage,
                    Category = category
                });
            }
            else
            {
                // Nếu sản phẩm đã có, chỉ tăng số lượng
                existingItem.Quantity += quantity;
            }
        }

        // Xóa sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        // Tính tổng giá trị giỏ hàng
        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        // Làm trống giỏ hàng
        public void Clear()
        {
            items.Clear();
        }

        // Cập nhật số lượng của một sản phẩm
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    RemoveItem(productId);
                }
            }
        }

        /* ----- Các thuộc tính cho Giỏ hàng nâng cao (slide 749) ----- */
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;
        public IPagedList<Product> SimilarProducts { get; set; }
    }
}
