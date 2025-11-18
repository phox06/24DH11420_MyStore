using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;


namespace _24DH11420_LTTH_BE234.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        public ActionResult Index(string SearchTerm, int? page) // <-- 1. Renamed 'searchTearm'
        {
            var model = new HomeProductVM();
            int pageNumber = (page ?? 1);
            int pageSize = model.PageSize;
            var products = db.Products.Include(p => p.Category).AsQueryable();
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                model.SearchTerm = SearchTerm;


                products = products.Where(p => p.ProductName.Contains(SearchTerm) ||
                                               p.ProductDescription.Contains(SearchTerm) ||
                                               p.Category.CategoryName.Contains(SearchTerm));
            }
            model.FeaturedProducts = products
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

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var category = db.Categories.Find(categoryId);
            if (category == null)
            {

                return HttpNotFound();
            }


            ViewBag.CategoryName = category.CategoryName;


            int pageSize = 6;
            int pageNumber = (page ?? 1);


            var products = db.Products
                .Where(p => p.CategoryID == categoryId)
                .OrderBy(p => p.ProductName);


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