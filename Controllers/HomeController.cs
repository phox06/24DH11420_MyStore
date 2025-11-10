using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.Data.Entity;
using PagedList;
using PagedList.Mvc;
using System.Net;


namespace _24DH11420_LTTH_BE234.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index(string searchTearm, int? page)
        {
            var model = new HomeProductVM();
            int pageNumber = (page ?? 1);
            int pageSize = model.PageSize;
            var products = db.Products.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrEmpty(searchTearm))
            {
                model.SearchTerm = searchTearm;
                products = products.Where(p => p.ProductName.Contains(searchTearm) ||
                                               p.ProductDescription.Contains(searchTearm) ||
                                               p.Category.CategoryName.Contains(searchTearm));
            }
            model.FeaturedProducts = db.Products
                                   .OrderByDescending(p => p.OrderDetails.Count())
                                   .Take(10)
                                   .ToList();
            model.NewProducts = products
                              .OrderBy(p => p.OrderDetails.Count())
                              .Take(20)
                              .ToPagedList(pageNumber, pageSize);

            return View(model);
           
        }
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); // Lỗi 400
            }
            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound(); // Lỗi 404
            }
            ProductDetailVM model = new ProductDetailVM();
            var products = db.Products
        .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID)
        .AsQueryable();

            
            int pageNumber = (page ?? 1);
            int pageSize = model.PageSize; 
            
            model.Product = pro;

            
            model.RelatedProducts = products
                .OrderBy(p => p.ProductID) 
                .Take(8)
                .ToPagedList(1, 8); 

            
            model.TopProducts = products
                .OrderByDescending(p => p.OrderDetails.Count())
                .Take(8)
                .ToPagedList(pageNumber, pageSize);

            
            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }

            
            model.estimatedValue = model.Product.ProductPrice * model.quantity;

            return View(model);
        }
        public ActionResult ProductList(int? categoryId, int? page)
        {
            if (categoryId == null)
            {
                // Nếu không có ID danh mục, trả về lỗi
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm danh mục để hiển thị tên
            var category = db.Categories.Find(categoryId);
            if (category == null)
            {
                // Nếu ID danh mục không tồn tại, trả về 404
                return HttpNotFound();
            }

            // Lưu tên danh mục để View có thể hiển thị
            ViewBag.CategoryName = category.CategoryName;

            // Cài đặt phân trang
            int pageSize = 6; // 6 sản phẩm mỗi trang
            int pageNumber = (page ?? 1);

            // Lấy tất cả sản phẩm thuộc danh mục này
            var products = db.Products
                .Where(p => p.CategoryID == categoryId)
                .OrderBy(p => p.ProductName); // Sắp xếp theo tên

            // Trả về một danh sách đã phân trang cho View
            return View(products.ToPagedList(pageNumber, pageSize));
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